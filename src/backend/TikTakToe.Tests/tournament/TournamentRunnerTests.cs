namespace TikTakToe.Tests.Tournament;

using TikTakToe.Tournament;

public class TournamentRunnerTests
{
    [Fact]
    public void RunTournament_ClassicalBeatsRandom_AndRanksHigher()
    {
        var config = new TournamentConfig
        {
            EngineIds = ["classical", "random"],
            GamesPerPair = 40,
            SearchDepth = 4,
            InitialElo = 1000.0,
            KFactor = 40.0,
            EloPasses = 50,
        };

        var runner = new TournamentRunner(config);
        var stats = runner.RunTournament(useParallelProcessing: false);

        Assert.Equal(2, stats.Count);

        var classical = stats.Single(s => s.EngineId == "classical");
        var random = stats.Single(s => s.EngineId == "random");

        Assert.True(classical.Wins > classical.Losses, $"Classical W/L was {classical.Wins}/{classical.Losses}");
        Assert.True(random.Losses > random.Wins, $"Random W/L was {random.Wins}/{random.Losses}");
        Assert.True(
            classical.CurrentElo > random.CurrentElo,
            $"Expected classical ({classical.CurrentElo:F1}) > random ({random.CurrentElo:F1})");
        Assert.True(classical.EloChange > 0);
        Assert.True(random.EloChange < 0);
        Assert.Equal("classical", stats[0].EngineId);

        // With correct win attribution, a strong engine should separate clearly from random.
        Assert.True(
            classical.CurrentElo - random.CurrentElo >= 80,
            $"Expected at least 80 Elo gap, got classical={classical.CurrentElo:F1} random={random.CurrentElo:F1} W/L {classical.Wins}/{classical.Losses} vs {random.Wins}/{random.Losses}");
    }

    [Fact]
    public void RunTournament_WinRateAndAverageMoves_AreHumanReadableFractions()
    {
        var config = new TournamentConfig
        {
            EngineIds = ["classical", "random"],
            GamesPerPair = 10,
            SearchDepth = 3,
            InitialElo = 1000.0,
        };

        var stats = new TournamentRunner(config).RunTournament(useParallelProcessing: false);
        var classical = stats.Single(s => s.EngineId == "classical");

        Assert.InRange(classical.WinRate, 0.0, 1.0);
        Assert.True(classical.AverageMovesPerGame > 0);
        Assert.True(classical.AverageMovesPerGame <= 9);
    }
}
