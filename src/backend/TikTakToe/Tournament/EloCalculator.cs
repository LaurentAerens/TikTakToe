namespace TikTakToe.Tournament;

/// <summary>
/// Calculates ELO ratings from completed match results.
/// Ratings are estimated only from the full result set (not updated live during play).
/// </summary>
public static class EloCalculator
{
    /// <summary>
    /// Default sensitivity for each batch step when fitting ratings to a result set.
    /// </summary>
    public const double DefaultKFactor = 40.0;

    /// <summary>
    /// Default number of batch passes used to fit ratings to the full result set.
    /// </summary>
    public const int DefaultPasses = 50;

    /// <summary>
    /// Calculates the expected score for a player based on their ELO rating vs opponent's rating.
    /// Expected score is a probability between 0 and 1.
    /// </summary>
    /// <returns></returns>
    public static double CalculateExpectedScore(double playerElo, double opponentElo)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (opponentElo - playerElo) / 400.0));
    }

    /// <summary>
    /// Updates ELO ratings based on a single match result (utility for unit tests / small demos).
    /// Tournament ratings should use <see cref="ComputeRatingsFromResults"/> instead.
    /// </summary>
    /// <returns></returns>
    public static double UpdateRating(double playerElo, double opponentElo, double actualScore, double kFactor = 32.0)
    {
        var expectedScore = CalculateExpectedScore(playerElo, opponentElo);
        return playerElo + (kFactor * (actualScore - expectedScore));
    }

    /// <summary>
    /// Converts a game result to an actual score for ELO calculation.
    /// </summary>
    /// <returns></returns>
    public static double ResultToScore(GameResult result)
    {
        return result switch
        {
            GameResult.Win => 1.0,
            GameResult.Draw => 0.5,
            GameResult.Loss => 0.0,
            _ => throw new ArgumentException($"Unknown game result: {result}", nameof(result)),
        };
    }

    /// <summary>
    /// Fits ELO ratings to a completed tournament result set.
    /// All games are known up front; no ratings are changed while games are still being played.
    /// </summary>
    /// <param name="engineIds">Engines that participated.</param>
    /// <param name="matchResults">Every finished game in the tournament.</param>
    /// <param name="initialElo">Starting rating for every engine (usually 1000).</param>
    /// <param name="kFactor">Step size for each batch pass.</param>
    /// <param name="passes">How many times to re-fit against the full result set.</param>
    /// <returns>Final rating per engine id.</returns>
    public static Dictionary<string, double> ComputeRatingsFromResults(
        IReadOnlyList<string> engineIds,
        IReadOnlyList<MatchResult> matchResults,
        double initialElo = 1000.0,
        double kFactor = DefaultKFactor,
        int passes = DefaultPasses)
    {
        var ratings = engineIds.ToDictionary(id => id, _ => initialElo);

        if (matchResults.Count == 0 || engineIds.Count == 0)
        {
            return ratings;
        }

        var gamesPlayed = engineIds.ToDictionary(id => id, _ => 0);
        foreach (var result in matchResults)
        {
            if (gamesPlayed.TryGetValue(result.Engine1Id, out var engine1Games))
            {
                gamesPlayed[result.Engine1Id] = engine1Games + 1;
            }

            if (gamesPlayed.TryGetValue(result.Engine2Id, out var engine2Games))
            {
                gamesPlayed[result.Engine2Id] = engine2Games + 1;
            }
        }

        var passCount = Math.Max(1, passes);
        for (var pass = 0; pass < passCount; pass++)
        {
            var deltas = engineIds.ToDictionary(id => id, _ => 0.0);

            // Freeze ratings for this pass so every game uses the same snapshot of the field.
            foreach (var result in matchResults)
            {
                if (!ratings.TryGetValue(result.Engine1Id, out var rating1)
                    || !ratings.TryGetValue(result.Engine2Id, out var rating2))
                {
                    continue;
                }

                var engine1Score = ResultToScore(result.ResultFromEngine1Perspective);
                var expected1 = CalculateExpectedScore(rating1, rating2);
                var residual1 = engine1Score - expected1;

                deltas[result.Engine1Id] += residual1;
                deltas[result.Engine2Id] -= residual1;
            }

            foreach (var engineId in engineIds)
            {
                var n = Math.Max(1, gamesPlayed[engineId]);
                ratings[engineId] += kFactor * (deltas[engineId] / n);
            }
        }

        return ratings;
    }
}
