namespace TikTakToe.ConsoleApp;

using System.Diagnostics;
using TikTakToe.Tournament;

internal static class TournamentMode
{
    /// <summary>
    /// Runs the tournament console flow.
    /// </summary>
    public static void Run()
    {
        Console.WriteLine("\n=== Tournament Mode ===\n");

        var selectedEngineIds = EngineMenu.PromptEngineIdsForTournament();
        if (selectedEngineIds.Count < 2)
        {
            Console.WriteLine("At least 2 engines are required for a tournament.");
            return;
        }

        var names = selectedEngineIds
            .Select(id => EngineRegistry.GetEngineInfo(id)?.Name ?? id);
        Console.WriteLine($"\nSelected engines: {string.Join(", ", names)}");

        var gamesPerPair = ConsoleIO.ReadInt("\nNumber of games per pair (default 10): ", 10);
        if (gamesPerPair < 1)
        {
            gamesPerPair = 10;
        }

        var searchDepth = ConsoleIO.ReadOptionalPositiveInt(
            "\nSearch depth for engines that support it (leave blank for full search): ");

        var config = new TournamentConfig
        {
            EngineIds = selectedEngineIds,
            GamesPerPair = gamesPerPair,
            SearchDepth = searchDepth,
            InitialElo = 1000.0,
            KFactor = EloCalculator.DefaultKFactor,
            EloPasses = EloCalculator.DefaultPasses,
        };

        var totalGames = (selectedEngineIds.Count * (selectedEngineIds.Count - 1) / 2) * gamesPerPair;

        Console.WriteLine("\n=== Starting Tournament ===");
        Console.WriteLine($"Engines: {selectedEngineIds.Count}");
        Console.WriteLine($"Games per pair: {gamesPerPair}");
        Console.WriteLine($"Total games: {totalGames}");
        Console.WriteLine($"Search depth: {searchDepth?.ToString() ?? "Full search"}");
        Console.WriteLine("ELO: computed once at the end from full results");
        Console.WriteLine("Parallel processing: Enabled");
        Console.WriteLine();

        var runner = new TournamentRunner(config);
        var stopwatch = Stopwatch.StartNew();

        var statistics = runner.RunTournament((completed, total) =>
        {
            var percentage = (double)completed / total * 100;
            Console.Write($"\rProgress: {completed}/{total} games ({percentage:F1}%)");
            Console.Out.Flush();
        });

        stopwatch.Stop();

        Console.WriteLine();
        Console.WriteLine($"\n=== Tournament Complete (took {stopwatch.Elapsed.TotalSeconds:F2}s) ===");
        Console.WriteLine("ELO fitted from full tournament results.\n");

        DisplayResults(statistics);
    }

    /// <summary>
    /// Displays the tournament results to the console.
    /// </summary>
    /// <param name="statistics">The collected engine statistics.</param>
    private static void DisplayResults(IReadOnlyList<EngineStatistics> statistics)
    {
        Console.WriteLine("=== Tournament Results ===\n");
        Console.WriteLine($"{"Rank",-5} {"Engine",-15} {"ELO",-8} {"Change",-8} {"W/L/D",-18} {"WinRate",-10} {"AvgMoves",-10}");
        Console.WriteLine(new string('-', 84));

        for (var i = 0; i < statistics.Count; i++)
        {
            var stats = statistics[i];
            var currentElo = (int)Math.Round(stats.CurrentElo);
            var eloChange = stats.EloChange >= 0
                ? $"+{(int)Math.Round(stats.EloChange)}"
                : $"{(int)Math.Round(stats.EloChange)}";
            var winLossDraw = $"{stats.Wins}/{stats.Losses}/{stats.Draws}";
            var winRate = $"{stats.WinRate * 100:F1}%";
            var avgMoves = $"{stats.AverageMovesPerGame:F1}";

            Console.WriteLine(
                $"{i + 1,-5} {stats.EngineName,-15} {currentElo,-8} {eloChange,-8} {winLossDraw,-18} {winRate,-10} {avgMoves,-10}");
        }

        Console.WriteLine();
        Console.WriteLine("Legend:");
        Console.WriteLine("  ELO: Rating fitted from the full tournament result set");
        Console.WriteLine("  Change: ELO change from initial rating");
        Console.WriteLine("  W/L/D: Wins/Losses/Draws");
        Console.WriteLine("  WinRate: Percentage of games won");
        Console.WriteLine("  AvgMoves: Average moves per game");
    }
}
