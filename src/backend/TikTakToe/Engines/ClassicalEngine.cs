namespace TikTakToe.Engines;

/// <summary>
/// Classical minimax engine for 3x3 Tic-Tac-Toe.
/// Defaults to full-resolution search (searches remaining empty squares) when no depth is provided.
/// The root expansion is evaluated in parallel to utilize multiple cores; deeper recursion is sequential.
/// When a search depth limit is hit during recursion, nodes return score 0 (non-terminal heuristic).
/// </summary>
public sealed class ClassicalEngine : MinimaxEngineBase
{
    protected override int EvalBoard(int[,] board)
    {
        // Fast center-first checks: many winning lines include the center cell.
        // Checking center ownership first is a small optimization.
        if (board[1, 1] == 1 &&
            (
                (board[0, 0] == 1 && board[2, 2] == 1) ||
                (board[0, 2] == 1 && board[2, 0] == 1) ||
                (board[0, 1] == 1 && board[2, 1] == 1) ||
                (board[1, 0] == 1 && board[1, 2] == 1)
            ))
        {
            return 1000;
        }

        if (board[1, 1] == 2 &&
            (
                (board[0, 0] == 2 && board[2, 2] == 2) ||
                (board[0, 2] == 2 && board[2, 0] == 2) ||
                (board[0, 1] == 2 && board[2, 1] == 2) ||
                (board[1, 0] == 2 && board[1, 2] == 2)
            ))
        {
            return -1000;
        }

        // Row and column wins (non-center dependent)
        if ((board[0,0] == 1 && board[0,1] == 1 && board[0,2] == 1) ||
            (board[1,0] == 1 && board[1,1] == 1 && board[1,2] == 1) ||
            (board[2,0] == 1 && board[2,1] == 1 && board[2,2] == 1) ||
            (board[0,0] == 1 && board[1,0] == 1 && board[2,0] == 1) ||
            (board[0,1] == 1 && board[1,1] == 1 && board[2,1] == 1) ||
            (board[0,2] == 1 && board[1,2] == 1 && board[2,2] == 1))
        {
            return 1000;
        }

        if ((board[0,0] == 2 && board[0,1] == 2 && board[0,2] == 2) ||
            (board[1,0] == 2 && board[1,1] == 2 && board[1,2] == 2) ||
            (board[2,0] == 2 && board[2,1] == 2 && board[2,2] == 2) ||
            (board[0,0] == 2 && board[1,0] == 2 && board[2,0] == 2) ||
            (board[0,1] == 2 && board[1,1] == 2 && board[2,1] == 2) ||
            (board[0,2] == 2 && board[1,2] == 2 && board[2,2] == 2))
        {
            return -1000;
        }
        return 0;
    }
}
