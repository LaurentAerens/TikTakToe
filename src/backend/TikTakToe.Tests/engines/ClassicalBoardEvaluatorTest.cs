namespace TikTakToe.Tests.Engines;

using System.Collections.Generic;
using TikTakToe.Engines.Evaluation;

public class ClassicalBoardEvaluatorTest
{
    public static IEnumerable<object[]> WinningBoards()
    {
        // Player 1 winning lines (expect 1000)
        yield return new object[]
        {
            new int[3, 3]
            {
                { 1, 1, 1 },
                { 0, 0, 0 },
                { 0, 0, 0 },
            },
            1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 0, 0 },
                { 1, 1, 1 },
                { 0, 0, 0 },
            },
            1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 1, 1, 1 },
            },
            1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 1, 0, 0 },
                { 1, 0, 0 },
                { 1, 0, 0 },
            },
            1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 1, 0 },
                { 0, 1, 0 },
                { 0, 1, 0 },
            },
            1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 0, 1 },
                { 0, 0, 1 },
                { 0, 0, 1 },
            },
            1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 1, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 1 },
            },
            1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 0, 1 },
                { 0, 1, 0 },
                { 1, 0, 0 },
            },
            1000,
        };

        // Player 2 winning lines (expect -1000)
        yield return new object[]
        {
            new int[3, 3]
            {
                { 2, 2, 2 },
                { 0, 0, 0 },
                { 0, 0, 0 },
            },
            -1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 0, 0 },
                { 2, 2, 2 },
                { 0, 0, 0 },
            },
            -1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 2, 2, 2 },
            },
            -1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 2, 0, 0 },
                { 2, 0, 0 },
                { 2, 0, 0 },
            },
            -1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 2, 0 },
                { 0, 2, 0 },
                { 0, 2, 0 },
            },
            -1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 0, 2 },
                { 0, 0, 2 },
                { 0, 0, 2 },
            },
            -1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 2, 0, 0 },
                { 0, 2, 0 },
                { 0, 0, 2 },
            },
            -1000,
        };
        yield return new object[]
        {
            new int[3, 3]
            {
                { 0, 0, 2 },
                { 0, 2, 0 },
                { 2, 0, 0 },
            },
            -1000,
        };
    }

    [Fact]
    public void Eval_Player1RowWin_Returns1000()
    {
        var evaluator = new ClassicalBoardEvaluator();
        var board = new int[3, 3]
        {
            { 1, 1, 1 },
            { 0, 2, 0 },
            { 0, 0, 2 },
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(1000, score);
    }

    [Fact]
    public void Eval_Player2DiagonalWin_ReturnsMinus1000()
    {
        var evaluator = new ClassicalBoardEvaluator();
        var board = new int[3, 3]
        {
            { 2, 1, 1 },
            { 0, 2, 0 },
            { 0, 0, 2 },
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(-1000, score);
    }

    [Fact]
    public void Eval_CenterAndOppositeCorners_Player1_Returns1000()
    {
        var evaluator = new ClassicalBoardEvaluator();
        var board = new int[3, 3]
        {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 },
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(1000, score);
    }

    [Fact]
    public void Eval_CenterAndVerticalSides_Player1_Returns1000()
    {
        var evaluator = new ClassicalBoardEvaluator();
        var board = new int[3, 3]
        {
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(1000, score);
    }

    [Fact]
    public void Eval_CenterAndOppositeCorners_Player2_ReturnsMinus1000()
    {
        var evaluator = new ClassicalBoardEvaluator();
        var board = new int[3, 3]
        {
            { 2, 0, 0 },
            { 0, 2, 0 },
            { 0, 0, 2 },
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(-1000, score);
    }

    [Fact]
    public void Eval_FullDrawBoard_Returns0()
    {
        var evaluator = new ClassicalBoardEvaluator();
        var board = new int[3, 3]
        {
            { 1, 2, 1 },
            { 2, 1, 2 },
            { 2, 1, 2 },
        };

        var score = evaluator.Evaluate(board);

        Assert.Equal(0, score);
    }

    [Theory]
    [MemberData(nameof(WinningBoards))]
    public void Eval_WinningBoards_ReturnsExpectedTerminalScore(int[,] board, int expected)
    {
        var evaluator = new ClassicalBoardEvaluator();
        var score = evaluator.Evaluate(board);

        Assert.Equal(expected, score);
    }
}
