namespace TikTakToe.Tournament;

/// <summary>
/// Orchestrates a tournament between multiple engines.
/// Games are played first; ELO is computed once at the end from the full result set.
/// </summary>
public class TournamentRunner
{
    private readonly TournamentConfig _config;
    private readonly List<MatchResult> _matchResults = [];
    private readonly object _lock = new();

    public TournamentRunner(TournamentConfig config)
    {
        this._config = config;
    }

    /// <summary>
    /// Runs the tournament and returns the final statistics for all engines.
    /// </summary>
    /// <param name="progressCallback">Optional callback for progress updates (current, total).</param>
    /// <param name="useParallelProcessing">Enable parallel processing for faster execution.</param>
    /// <returns></returns>
    public List<EngineStatistics> RunTournament(Action<int, int>? progressCallback = null, bool useParallelProcessing = true)
    {
        this._matchResults.Clear();

        var pairings = this.GeneratePairings();
        var totalGames = pairings.Count * this._config.GamesPerPair;
        var completedGames = 0;

        // Phase 1: play every game. No ratings are updated during this phase.
        if (useParallelProcessing && pairings.Count > 1)
        {
            Parallel.ForEach(pairings, pairing =>
            {
                this.RunMatchesForPairing(pairing, gameCompleted =>
                {
                    lock (this._lock)
                    {
                        completedGames += gameCompleted;
                        progressCallback?.Invoke(completedGames, totalGames);
                    }
                });
            });
        }
        else
        {
            foreach (var pairing in pairings)
            {
                this.RunMatchesForPairing(pairing, gameCompleted =>
                {
                    completedGames += gameCompleted;
                    progressCallback?.Invoke(completedGames, totalGames);
                });
            }
        }

        // Phase 2: fit ELO to the complete tournament result set only.
        var eloRatings = EloCalculator.ComputeRatingsFromResults(
            this._config.EngineIds,
            this._matchResults,
            this._config.InitialElo,
            this._config.KFactor,
            this._config.EloPasses);

        return this.CalculateStatistics(eloRatings);
    }

    /// <summary>
    /// Gets all match results from the tournament.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<MatchResult> GetMatchResults()
    {
        return this._matchResults.AsReadOnly();
    }

    private List<(string Engine1, string Engine2)> GeneratePairings()
    {
        var pairings = new List<(string, string)>();
        var engines = this._config.EngineIds;

        for (var i = 0; i < engines.Count; i++)
        {
            for (var j = i + 1; j < engines.Count; j++)
            {
                pairings.Add((engines[i], engines[j]));
            }
        }

        return pairings;
    }

    private int RunMatchesForPairing((string Engine1, string Engine2) pairing, Action<int>? gameProgressCallback = null)
    {
        var engine1 = EngineRegistry.CreateEngine(pairing.Engine1);
        var engine2 = EngineRegistry.CreateEngine(pairing.Engine2);
        var localResults = new List<MatchResult>();

        for (var game = 0; game < this._config.GamesPerPair; game++)
        {
            // Alternate who plays first to be fair
            var (firstEngine, secondEngine) = game % 2 == 0
                ? (engine1, engine2)
                : (engine2, engine1);

            var result = GameRunner.RunGame(firstEngine, secondEngine, this._config.SearchDepth);
            localResults.Add(result);

            gameProgressCallback?.Invoke(1);
        }

        lock (this._lock)
        {
            this._matchResults.AddRange(localResults);
        }

        return this._config.GamesPerPair;
    }

    private List<EngineStatistics> CalculateStatistics(IReadOnlyDictionary<string, double> eloRatings)
    {
        var statistics = new List<EngineStatistics>();

        foreach (var engineId in this._config.EngineIds)
        {
            var engineInfo = EngineRegistry.GetEngineInfo(engineId);
            var engineName = engineInfo?.Name ?? engineId;

            var engineResults = this._matchResults.Where(r =>
                r.Engine1Id == engineId || r.Engine2Id == engineId).ToList();

            var wins = 0;
            var losses = 0;
            var draws = 0;
            var totalMoves = 0;

            foreach (var result in engineResults)
            {
                if (result.Engine1Id == engineId)
                {
                    switch (result.ResultFromEngine1Perspective)
                    {
                        case GameResult.Win:
                            wins++;
                            break;
                        case GameResult.Loss:
                            losses++;
                            break;
                        case GameResult.Draw:
                            draws++;
                            break;
                    }
                }
                else
                {
                    // Result is from engine1's perspective, so invert for engine2
                    switch (result.ResultFromEngine1Perspective)
                    {
                        case GameResult.Win:
                            losses++;
                            break;
                        case GameResult.Loss:
                            wins++;
                            break;
                        case GameResult.Draw:
                            draws++;
                            break;
                    }
                }

                totalMoves += result.MoveCount;
            }

            statistics.Add(new EngineStatistics
            {
                EngineId = engineId,
                EngineName = engineName,
                GamesPlayed = engineResults.Count,
                Wins = wins,
                Losses = losses,
                Draws = draws,
                InitialElo = this._config.InitialElo,
                CurrentElo = eloRatings[engineId],
                TotalMoves = totalMoves,
            });
        }

        return statistics.OrderByDescending(s => s.CurrentElo).ToList();
    }
}
