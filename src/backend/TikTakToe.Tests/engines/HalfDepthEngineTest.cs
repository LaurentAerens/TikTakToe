using TikTakToe.Engines.Evaluation;

namespace TikTakToe.Tests.Engines;

public class HalfDepthBoardEvaluatorTest
{
    [Fact]
    public void Eval_Player1TerminalWin_Returns1000()
    {
        var evaluator = new HalfDepthBoardEvaluator();
        var board = new int[3, 3]
        {
            { 1, 1, 1 },
            { 0, 2, 0 },
            { 2, 0, 0 }
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(1000, score);
    }

    [Fact]
    public void Eval_Player2TerminalWin_ReturnsMinus1000()
    {
        var evaluator = new HalfDepthBoardEvaluator();
        var board = new int[3, 3]
        {
            { 2, 2, 2 },
            { 0, 1, 0 },
            { 1, 0, 0 }
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(-1000, score);
    }

    [Fact]
    public void Eval_Player1TwoInRowThreat_ReturnsPositiveHeuristic()
    {
        var evaluator = new HalfDepthBoardEvaluator();
        var board = new int[3, 3]
        {
            { 1, 1, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(500, score);
    }

    [Fact]
    public void Eval_Player2TwoInRowThreat_ReturnsNegativeHeuristic()
    {
        var evaluator = new HalfDepthBoardEvaluator();
        var board = new int[3, 3]
        {
            { 2, 2, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(-500, score);
    }

    [Fact]
    public void Eval_MultiplePlayer1Threats_IsClampedTo1000()
    {
        var evaluator = new HalfDepthBoardEvaluator();
        var board = new int[3, 3]
        {
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 0, 0, 0 }
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(1000, score);
    }

    [Fact]
    public void Eval_OverlappingThreatsOnSameSquare_CountsOnce()
    {
        var evaluator = new HalfDepthBoardEvaluator();
        var board = new int[3, 3]
        {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 0, 1, 0 }
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(500, score);
    }

    [Fact]
    public void Eval_TwoThreatsOnDifferentSquares_ScoresAs1000()
    {
        var evaluator = new HalfDepthBoardEvaluator();
        var board = new int[3, 3]
        {
            { 1, 1, 0 },
            { 0, 0, 0 },
            { 1, 1, 0 }
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(1000, score);
    }

    [Fact]
    public void Eval_NonTerminalBoard_ReturnsHeuristicInRange()
    {
        var evaluator = new HalfDepthBoardEvaluator();
        var board = new int[3, 3]
        {
            { 1, 0, 0 },
            { 0, 2, 0 },
            { 0, 0, 0 }
        };

        var score = evaluator.Evaluate(board);

        Assert.InRange(score, -1000, 1000);
    }
}
