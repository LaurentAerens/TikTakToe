using TikTakToe.Engines;
using TikTakToe.Engines.Exceptions;

namespace TikTakToe.Tests.Engines;

public class HalfDepthEngineTest
{
    [Fact]
    public void Eval_Player1TerminalWin_Returns1000()
    {
        var engine = new HalfDepthEngine();
        var board = new int[3, 3]
        {
            { 1, 1, 1 },
            { 0, 2, 0 },
            { 2, 0, 0 }
        };

        var score = engine.Eval(board, player: 1);

        Assert.Equal(1000, score);
    }

    [Fact]
    public void Eval_Player2TerminalWin_ReturnsMinus1000()
    {
        var engine = new HalfDepthEngine();
        var board = new int[3, 3]
        {
            { 2, 2, 2 },
            { 0, 1, 0 },
            { 1, 0, 0 }
        };

        var score = engine.Eval(board, player: 1);

        Assert.Equal(-1000, score);
    }

    [Fact]
    public void Eval_Player1TwoInRowThreat_ReturnsPositiveHeuristic()
    {
        var engine = new HalfDepthEngine();
        var board = new int[3, 3]
        {
            { 1, 1, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        var score = engine.Eval(board, player: 1);

        Assert.Equal(500, score);
    }

    [Fact]
    public void Eval_Player2TwoInRowThreat_ReturnsNegativeHeuristic()
    {
        var engine = new HalfDepthEngine();
        var board = new int[3, 3]
        {
            { 2, 2, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        var score = engine.Eval(board, player: 1);

        Assert.Equal(-500, score);
    }

    [Fact]
    public void Eval_MultiplePlayer1Threats_IsClampedTo1000()
    {
        var engine = new HalfDepthEngine();
        var board = new int[3, 3]
        {
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 0, 0, 0 }
        };

        var score = engine.Eval(board, player: 1);

        Assert.Equal(1000, score);
    }

    [Fact]
    public void Eval_NonStandardBoard_ThrowsBoardSizeNotSupportedException()
    {
        var engine = new HalfDepthEngine();
        var board = new int[4, 4];

        var ex = Record.Exception(() => engine.Eval(board, player: 1));

        Assert.NotNull(ex);
        Assert.IsType<BoardSizeNotSupportedException>(ex);
    }

    [Fact]
    public void Move_WithSingleAvailableMove_ReturnsThatMove()
    {
        var engine = new HalfDepthEngine();
        var board = new int[3, 3]
        {
            { 1, 2, 1 },
            { 2, 1, 2 },
            { 2, 1, 0 }
        };

        var (updatedBoard, score) = engine.Move(board, player: 2);

        Assert.Equal(2, updatedBoard[2, 2]);
        Assert.InRange(score, -1000, 1000);
    }

    [Fact]
    public void Eval_WithDepth_ReturnsScoreInRange()
    {
        var engine = new HalfDepthEngine();
        var board = new int[3, 3]
        {
            { 1, 0, 0 },
            { 0, 2, 0 },
            { 0, 0, 0 }
        };

        var score = engine.Eval(board, player: 1, depth: 2);

        Assert.InRange(score, -1000, 1000);
    }
}
