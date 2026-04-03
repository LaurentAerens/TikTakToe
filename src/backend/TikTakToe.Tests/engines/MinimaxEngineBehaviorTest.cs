using TikTakToe.Engines;
using TikTakToe.Engines.Exceptions;
using TikTakToe.Engines.Interface;

namespace TikTakToe.Tests.Engines;

public class MinimaxEngineBehaviorTest
{
    public static IEnumerable<object[]> MinimaxEngineFactories()
    {
        yield return new object[] { new Func<IEngine>(() => new ClassicalEngine()) };
        yield return new object[] { new Func<IEngine>(() => new HalfDepthEngine()) };
        yield return new object[] { new Func<IEngine>(() => new ClassicalExpectimaxEngine()) };
        yield return new object[] { new Func<IEngine>(() => new HalfDepthExpectimaxEngine()) };
    }

    [Theory]
    [MemberData(nameof(MinimaxEngineFactories))]
    public void Eval_WithDepthZero_Returns0_ForNonTerminalBoard(Func<IEngine> factory)
    {
        var engine = factory();
        var board = new int[3, 3]
        {
            { 1, 0, 0 },
            { 0, 2, 0 },
            { 0, 0, 0 }
        };

        var score = engine.Eval(board, player: 1, depth: 0);

        Assert.Equal(0, score);
    }

    [Theory]
    [MemberData(nameof(MinimaxEngineFactories))]
    public void Eval_NonStandardBoard_ThrowsBoardSizeNotSupportedException(Func<IEngine> factory)
    {
        var engine = factory();
        var board = new int[4, 4]
        {
            { 1, 2, 1, 2 },
            { 2, 1, 0, 1 },
            { 1, 2, 2, 1 },
            { 2, 1, 1, 2 }
        };

        var ex = Record.Exception(() => engine.Eval(board, player: 1));

        Assert.NotNull(ex);
        Assert.IsType<BoardSizeNotSupportedException>(ex);
    }

    [Theory]
    [MemberData(nameof(MinimaxEngineFactories))]
    public void Move_WithFullBoard_ThrowsNoMoveAvailableException(Func<IEngine> factory)
    {
        var engine = factory();
        var fullBoard = new int[3, 3]
        {
            { 1, 2, 1 },
            { 2, 1, 2 },
            { 2, 1, 2 }
        };

        var ex = Record.Exception(() => engine.Move(fullBoard, player: 1));

        Assert.NotNull(ex);
        Assert.IsType<NoMoveAvailableException>(ex);
    }

    [Theory]
    [MemberData(nameof(MinimaxEngineFactories))]
    public void Move_WithSingleAvailableMove_ReturnsThatMoveAndScore(Func<IEngine> factory)
    {
        var engine = factory();
        var board = new int[3, 3]
        {
            { 1, 2, 1 },
            { 2, 1, 2 },
            { 2, 1, 0 }
        };

        var (updatedBoard, score) = engine.Move(board, player: 2);

        var changed = 0;
        for (var x = 0; x < 3; x++)
        {
            for (var y = 0; y < 3; y++)
            {
                if (board[x, y] != updatedBoard[x, y])
                {
                    changed++;
                    Assert.Equal(0, board[x, y]);
                    Assert.Equal(2, updatedBoard[x, y]);
                }
            }
        }

        Assert.Equal(1, changed);
        Assert.InRange(score, -1000, 1000);
    }

    [Theory]
    [MemberData(nameof(MinimaxEngineFactories))]
    public void Move_PicksImmediateWin_ForPlayer1(Func<IEngine> factory)
    {
        var engine = factory();
        var board = new int[3, 3]
        {
            { 1, 1, 0 },
            { 2, 2, 0 },
            { 0, 0, 0 }
        };

        var (updatedBoard, score) = engine.Move(board, player: 1);

        Assert.Equal(1000, score);
        Assert.Equal(1, updatedBoard[0, 2]);
    }

    [Theory]
    [MemberData(nameof(MinimaxEngineFactories))]
    public void Move_PicksImmediateWin_ForPlayer2(Func<IEngine> factory)
    {
        var engine = factory();
        var board = new int[3, 3]
        {
            { 2, 2, 0 },
            { 1, 1, 0 },
            { 0, 0, 0 }
        };

        var (updatedBoard, score) = engine.Move(board, player: 2);

        Assert.Equal(-1000, score);
        Assert.Equal(2, updatedBoard[0, 2]);
    }

    [Theory]
    [MemberData(nameof(MinimaxEngineFactories))]
    public void Eval_FullBoard_WithDepth_Returns0_MinimaxNoMovesBranch(Func<IEngine> factory)
    {
        var engine = factory();
        var fullBoard = new int[3, 3]
        {
            { 1, 2, 1 },
            { 2, 1, 2 },
            { 2, 1, 2 }
        };

        var score = engine.Eval(fullBoard, player: 1, depth: 1);

        Assert.Equal(0, score);
    }
}
