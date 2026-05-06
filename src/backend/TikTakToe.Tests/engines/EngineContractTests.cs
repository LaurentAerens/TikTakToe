namespace TikTakToe.Tests;

using TikTakToe.Engines.Exceptions;
using TikTakToe.Engines.Interface;

public class EngineContractTests
{
    public static IEnumerable<object[]> EngineTypes()
    {
        var engineTypes = typeof(IEngine).Assembly
            .GetTypes()
            .Where(t => typeof(IEngine).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetConstructor(Type.EmptyTypes) is not null)
            .ToArray();

        Assert.NotEmpty(engineTypes);

        foreach (var engineType in engineTypes)
        {
            yield return new object[] { engineType };
        }
    }

    [Theory]
    [MemberData(nameof(EngineTypes))]
    public void Move_WithValidBoard_ReturnsBoardWithOneMoveAndScoreInRange(Type engineType)
    {
        var engine = CreateEngine(engineType);
        var player = 1;
        var board = new int[3, 3]
        {
            { 1, 2, 0 },
            { 0, 1, 2 },
            { 2, 0, 1 },
        };

        var (updatedBoard, score) = engine.Move(board, player);

        Assert.Equal(board.GetLength(0), updatedBoard.GetLength(0));
        Assert.Equal(board.GetLength(1), updatedBoard.GetLength(1));
        Assert.InRange(score, -1000, 1000);

        var changedPositions = 0;
        for (var x = 0; x < board.GetLength(0); x++)
        {
            for (var y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] != updatedBoard[x, y])
                {
                    changedPositions++;
                    Assert.Equal(0, board[x, y]);
                    Assert.Equal(player, updatedBoard[x, y]);
                }
            }
        }

        Assert.Equal(1, changedPositions);
    }

    [Theory]
    [MemberData(nameof(EngineTypes))]
    public void Eval_ReturnsScoreInRange(Type engineType)
    {
        var engine = CreateEngine(engineType);
        var board = new int[3, 3]
        {
            { 1, 2, 0 },
            { 0, 1, 2 },
            { 2, 0, 1 },
        };

        var score = engine.Eval(board, player: 1);

        Assert.InRange(score, -1000, 1000);
    }

    [Theory]
    [MemberData(nameof(EngineTypes))]
    public void Move_WithFullBoard_ThrowsNoMoveAvailableException(Type engineType)
    {
        var engine = CreateEngine(engineType);
        var fullBoard = new int[3, 3]
        {
            { 1, 2, 1 },
            { 2, 1, 2 },
            { 2, 1, 2 },
        };

        var ex = Record.Exception(() => engine.Move(fullBoard, player: 1));

        Assert.NotNull(ex);
        Assert.IsType<NoMoveAvailableException>(ex);
    }

    [Theory]
    [MemberData(nameof(EngineTypes))]
    public void Eval_WithDepth_EitherReturnsScoreInRangeOrThrowsUnsupportedDepthException(Type engineType)
    {
        var engine = CreateEngine(engineType);
        var board = new int[3, 3]
        {
            { 1, 2, 0 },
            { 0, 1, 2 },
            { 2, 0, 1 },
        };

        try
        {
            var score = engine.Eval(board, player: 1, depth: 3);
            Assert.InRange(score, -1000, 1000);
        }
        catch (Exception ex)
        {
            Assert.IsType<UnsupportedDepthException>(ex);
        }
    }

    [Theory]
    [MemberData(nameof(EngineTypes))]
    public void Move_WithDepth_EitherReturnsValidResultOrThrowsUnsupportedDepthException(Type engineType)
    {
        var engine = CreateEngine(engineType);
        var player = 2;
        var board = new int[3, 3]
        {
            { 1, 2, 0 },
            { 0, 1, 2 },
            { 2, 0, 1 },
        };

        try
        {
            var (updatedBoard, score) = engine.Move(board, player, depth: 3);
            Assert.InRange(score, -1000, 1000);

            var changedPositions = 0;
            for (var x = 0; x < board.GetLength(0); x++)
            {
                for (var y = 0; y < board.GetLength(1); y++)
                {
                    if (board[x, y] != updatedBoard[x, y])
                    {
                        changedPositions++;
                        Assert.Equal(0, board[x, y]);
                        Assert.Equal(player, updatedBoard[x, y]);
                    }
                }
            }

            Assert.Equal(1, changedPositions);
        }
        catch (Exception ex)
        {
            Assert.IsType<UnsupportedDepthException>(ex);
        }
    }

    [Theory]
    [MemberData(nameof(EngineTypes))]
    public void Move_WithNonStandardBoard_EitherReturnsValidResultOrThrowsBoardSizeNotSupportedException(Type engineType)
    {
        var engine = CreateEngine(engineType);
        var player = 1;
        var board = new int[4, 4]
        {
            { 1, 2, 1, 2 },
            { 2, 1, 0, 1 },
            { 1, 2, 2, 1 },
            { 2, 1, 1, 2 },
        };

        try
        {
            var (updatedBoard, score) = engine.Move(board, player);

            Assert.Equal(board.GetLength(0), updatedBoard.GetLength(0));
            Assert.Equal(board.GetLength(1), updatedBoard.GetLength(1));
            Assert.InRange(score, -1000, 1000);

            var changedPositions = 0;
            for (var x = 0; x < board.GetLength(0); x++)
            {
                for (var y = 0; y < board.GetLength(1); y++)
                {
                    if (board[x, y] != updatedBoard[x, y])
                    {
                        changedPositions++;
                        Assert.Equal(0, board[x, y]);
                        Assert.Equal(player, updatedBoard[x, y]);
                    }
                }
            }

            Assert.Equal(1, changedPositions);
        }
        catch (Exception ex)
        {
            Assert.IsType<BoardSizeNotSupportedException>(ex);
        }
    }

    private static IEngine CreateEngine(Type engineType)
    {
        return (IEngine)Activator.CreateInstance(engineType)!;
    }
}
