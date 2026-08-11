namespace TikTakToe.Tests.Tournament;

using TikTakToe.Tournament;

// Engine registry / board helpers covered in sibling tests.
public class EloCalculatorTests
{
    [Fact]
    public void CalculateExpectedScore_EqualRatings_IsHalf()
    {
        var expected = EloCalculator.CalculateExpectedScore(1000, 1000);
        Assert.Equal(0.5, expected, precision: 10);
    }

    [Fact]
    public void CalculateExpectedScore_HigherRatedPlayer_HasHigherExpectation()
    {
        var expected = EloCalculator.CalculateExpectedScore(1400, 1000);
        Assert.True(expected > 0.5);
        Assert.True(expected < 1.0);
    }

    [Fact]
    public void UpdateRating_WinAgainstEqualOpponent_IncreasesRating()
    {
        var updated = EloCalculator.UpdateRating(1000, 1000, actualScore: 1.0, kFactor: 32);
        Assert.Equal(1016, updated, precision: 5);
    }

    [Fact]
    public void UpdateRating_LossAgainstEqualOpponent_DecreasesRating()
    {
        var updated = EloCalculator.UpdateRating(1000, 1000, actualScore: 0.0, kFactor: 32);
        Assert.Equal(984, updated, precision: 5);
    }

    [Fact]
    public void UpdateRating_WinAndLossAgainstEqual_AreSymmetric()
    {
        var winGain = EloCalculator.UpdateRating(1000, 1000, 1.0, 32) - 1000;
        var lossDrop = 1000 - EloCalculator.UpdateRating(1000, 1000, 0.0, 32);
        Assert.Equal(winGain, lossDrop, precision: 10);
    }

    [Theory]
    [InlineData(GameResult.Win, 1.0)]
    [InlineData(GameResult.Draw, 0.5)]
    [InlineData(GameResult.Loss, 0.0)]
    public void ResultToScore_MapsResultsCorrectly(GameResult result, double expectedScore)
    {
        Assert.Equal(expectedScore, EloCalculator.ResultToScore(result));
    }

    [Fact]
    public void ComputeRatingsFromResults_UsesFullResultSet_StrongerEngineRanksHigher()
    {
        // A beats B in every decisive game (mixed colors).
        var results = new List<MatchResult>
        {
            new("a", "b", GameResult.Win, 5),
            new("b", "a", GameResult.Loss, 6),
            new("a", "b", GameResult.Win, 7),
            new("b", "a", GameResult.Loss, 5),
            new("a", "b", GameResult.Draw, 9),
            new("b", "a", GameResult.Draw, 9),
        };

        var ratings = EloCalculator.ComputeRatingsFromResults(
            ["a", "b"],
            results,
            initialElo: 1000,
            kFactor: 40,
            passes: 50);

        Assert.True(ratings["a"] > ratings["b"]);
        Assert.True(ratings["a"] > 1000);
        Assert.True(ratings["b"] < 1000);
    }

    [Fact]
    public void ComputeRatingsFromResults_EmptyResults_KeepsInitialElo()
    {
        var ratings = EloCalculator.ComputeRatingsFromResults(
            ["a", "b"],
            [],
            initialElo: 1200);

        Assert.Equal(1200, ratings["a"]);
        Assert.Equal(1200, ratings["b"]);
    }
}
