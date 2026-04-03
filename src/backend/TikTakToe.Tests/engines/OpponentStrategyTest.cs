using TikTakToe.Engines.Search;

namespace TikTakToe.Tests.Engines;

public class OpponentStrategyTest
{
    [Fact]
    public void AggregateScores_MinimaxOpponentTurn_Player1Engine_ReturnsWorstChild()
    {
        var strategy = new MinimaxOpponentStrategy();
        var scores = new[] { 1000, 0, -1000 };

        var aggregate = strategy.AggregateScores(scores, currentPlayer: 2, enginePlayer: 1);

        Assert.Equal(-1000, aggregate);
    }

    [Fact]
    public void AggregateScores_OppertunityOpponentTurn_Player1Engine_ReturnsAverageChild()
    {
        var strategy = new OppertunityOpponentStrategy();
        var scores = new[] { 1000, 0, -1000 };

        var aggregate = strategy.AggregateScores(scores, currentPlayer: 2, enginePlayer: 1);

        Assert.Equal(0, aggregate);
    }

    [Fact]
    public void AggregateScores_OppertunityEngineTurn_Player2Engine_StillChoosesBestForEngine()
    {
        var strategy = new OppertunityOpponentStrategy();
        var scores = new[] { -1000, -500, 500 };

        var aggregate = strategy.AggregateScores(scores, currentPlayer: 2, enginePlayer: 2);

        Assert.Equal(-1000, aggregate);
    }
}
