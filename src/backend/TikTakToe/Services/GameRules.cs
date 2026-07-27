namespace TikTakToe.Services;

/// <summary>
/// Helper class to check game rules, wins, and draws on rectangular boards.
/// </summary>
public static class GameRules
{
    /// <summary>
    /// Checks the board to find if any player has won the game.
    /// Returns the player number (e.g. 1 or 2) if a winner exists; otherwise null.
    /// </summary>
    /// <param name="board">The board state.</param>
    /// <returns>The winner's value or null.</returns>
    public static int? GetWinner(int[,]? board)
    {
        if (board is null)
        {
            return null;
        }

        var rows = board.GetLength(0);
        var cols = board.GetLength(1);

        // Standard Tic-Tac-Toe requires 3 in a row.
        // For boards smaller than 3 in both directions, the target is the minimum dimension.
        var target = rows >= 3 && cols >= 3 ? 3 : Math.Min(rows, cols);

        if (target <= 0)
        {
            return null;
        }

        // Horizontal, Vertical, Diagonal Down-Right, Diagonal Up-Right
        int[] dx = [0, 1, 1, 1];
        int[] dy = [1, 0, 1, -1];

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                var player = board[r, c];
                if (player == 0)
                {
                    continue;
                }

                for (var d = 0; d < 4; d++)
                {
                    var count = 1;
                    for (var step = 1; step < target; step++)
                    {
                        var nr = r + (dx[d] * step);
                        var nc = c + (dy[d] * step);
                        if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && board[nr, nc] == player)
                        {
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (count == target)
                    {
                        return player;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if the board is completely filled.
    /// </summary>
    /// <param name="board">The board state.</param>
    /// <returns>True if full; otherwise false.</returns>
    public static bool IsBoardFull(int[,]? board)
    {
        if (board is null)
        {
            return true;
        }

        var rows = board.GetLength(0);
        var cols = board.GetLength(1);

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                if (board[r, c] == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the game is over (either someone won, or the board is full).
    /// </summary>
    /// <param name="board">The board state.</param>
    /// <returns>True if game over; otherwise false.</returns>
    public static bool IsGameOver(int[,]? board)
    {
        if (board is null)
        {
            return true;
        }

        return GetWinner(board).HasValue || IsBoardFull(board);
    }
}
