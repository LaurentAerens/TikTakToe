namespace TikTakToe.Engines.Evaluation;

public sealed class HalfDepthBoardEvaluator : IBoardEvaluator
{
    public int Evaluate(int[,] board)
    {
        var terminal = BoardEvaluationPrimitives.EvaluateTerminalState(board);
        if (terminal != 0)
        {
            return terminal;
        }

        var score = 0;

        if (board[1, 1] == 0)
        {
            if ((board[0, 0] == 1 && board[2, 2] == 1) ||
                (board[0, 2] == 1 && board[2, 0] == 1) ||
                (board[0, 1] == 1 && board[2, 1] == 1) ||
                (board[1, 0] == 1 && board[1, 2] == 1))
            {
                score += 500;
            }

            if ((board[0, 0] == 2 && board[2, 2] == 2) ||
                (board[0, 2] == 2 && board[2, 0] == 2) ||
                (board[0, 1] == 2 && board[2, 1] == 2) ||
                (board[1, 0] == 2 && board[1, 2] == 2))
            {
                score -= 500;
            }
        }

        // Rows
        score += TwoInRowScore(board[0, 0], board[0, 1], board[0, 2]);
        score += TwoInRowScore(board[1, 0], board[1, 1], board[1, 2]);
        score += TwoInRowScore(board[2, 0], board[2, 1], board[2, 2]);

        // Columns
        score += TwoInRowScore(board[0, 0], board[1, 0], board[2, 0]);
        score += TwoInRowScore(board[0, 1], board[1, 1], board[2, 1]);
        score += TwoInRowScore(board[0, 2], board[1, 2], board[2, 2]);

        return Math.Clamp(score, -1000, 1000);
    }

    private static int TwoInRowScore(int a, int b, int c)
    {
        if ((a == 0 && b == 1 && c == 1) ||
            (a == 1 && b == 0 && c == 1) ||
            (a == 1 && b == 1 && c == 0))
        {
            return 500;
        }

        if ((a == 0 && b == 2 && c == 2) ||
            (a == 2 && b == 0 && c == 2) ||
            (a == 2 && b == 2 && c == 0))
        {
            return -500;
        }

        return 0;
    }
}