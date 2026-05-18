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

        if (board[0, 0] == 0)
        {
            if ((board[0, 1] == 1 && board[0, 2] == 1) ||
                (board[1, 0] == 1 && board[2, 0] == 1))
            {
                score += 500;
            }

            if ((board[0, 1] == 2 && board[0, 2] == 2) ||
                (board[1, 0] == 2 && board[2, 0] == 2))
            {
                score -= 500;
            }
        }

        if (board[0, 1] == 0)
        {
            if ((board[0, 0] == 1 && board[0, 2] == 1) ||
                (board[1, 1] == 1 && board[2, 1] == 1))
            {
                score += 500;
            }

            if ((board[0, 0] == 2 && board[0, 2] == 2) ||
                (board[1, 1] == 2 && board[2, 1] == 2))
            {
                score -= 500;
            }
        }

        if (board[0, 2] == 0)
        {
            if ((board[0, 0] == 1 && board[0, 1] == 1) ||
                (board[1, 2] == 1 && board[2, 2] == 1))
            {
                score += 500;
            }

            if ((board[0, 0] == 2 && board[0, 1] == 2) ||
                (board[1, 2] == 2 && board[2, 2] == 2))
            {
                score -= 500;
            }
        }

        if (board[1, 0] == 0)
        {
            if ((board[0, 0] == 1 && board[2, 0] == 1) ||
                (board[1, 1] == 1 && board[1, 2] == 1))
            {
                score += 500;
            }

            if ((board[0, 0] == 2 && board[2, 0] == 2) ||
                (board[1, 1] == 2 && board[1, 2] == 2))
            {
                score -= 500;
            }
        }

        if (board[1, 2] == 0)
        {
            if ((board[0, 2] == 1 && board[2, 2] == 1) ||
                (board[1, 0] == 1 && board[1, 1] == 1))
            {
                score += 500;
            }

            if ((board[0, 2] == 2 && board[2, 2] == 2) ||
                (board[1, 0] == 2 && board[1, 1] == 2))
            {
                score -= 500;
            }
        }

        if (board[2, 0] == 0)
        {
            if ((board[0, 0] == 1 && board[1, 0] == 1) ||
                (board[2, 1] == 1 && board[2, 2] == 1))
            {
                score += 500;
            }

            if ((board[0, 0] == 2 && board[1, 0] == 2) ||
                (board[2, 1] == 2 && board[2, 2] == 2))
            {
                score -= 500;
            }
        }

        if (board[2, 1] == 0)
        {
            if ((board[0, 1] == 1 && board[1, 1] == 1) ||
                (board[2, 0] == 1 && board[2, 2] == 1))
            {
                score += 500;
            }

            if ((board[0, 1] == 2 && board[1, 1] == 2) ||
                (board[2, 0] == 2 && board[2, 2] == 2))
            {
                score -= 500;
            }
        }

        if (board[2, 2] == 0)
        {
            if ((board[0, 2] == 1 && board[1, 2] == 1) ||
                (board[2, 0] == 1 && board[2, 1] == 1))
            {
                score += 500;
            }

            if ((board[0, 2] == 2 && board[1, 2] == 2) ||
                (board[2, 0] == 2 && board[2, 1] == 2))
            {
                score -= 500;
            }
        }

        return Math.Clamp(score, -1000, 1000);
    }
}
