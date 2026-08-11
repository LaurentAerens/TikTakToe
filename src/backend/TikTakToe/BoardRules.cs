namespace TikTakToe;

/// <summary>
/// Shared 3x3 board helpers used by the API, tournament runner, and console.
/// </summary>
public static class BoardRules
{
    /// <summary>
    /// Gets the board size for the tic-tac-toe grid.
    /// </summary>
    public const int Size = 3;

    /// <summary>
    /// Creates a new empty 3x3 board.
    /// </summary>
    /// <returns>An empty board initialized with zeros.</returns>
    public static int[,] CreateEmpty()
    {
        return new int[Size, Size]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };
    }

    /// <summary>
    /// Determines whether the specified player has a winning line on the board.
    /// </summary>
    /// <param name="board">The current board state.</param>
    /// <param name="player">The player identifier to check.</param>
    /// <returns><see langword="true"/> when the player has a winning line; otherwise, <see langword="false"/>.</returns>
    public static bool PlayerHasWon(int[,] board, int player)
    {
        for (var i = 0; i < Size; i++)
        {
            if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player)
            {
                return true;
            }

            if (board[0, i] == player && board[1, i] == player && board[2, i] == player)
            {
                return true;
            }
        }

        if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player)
        {
            return true;
        }

        if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the board is full.
    /// </summary>
    /// <param name="board">The current board state.</param>
    /// <returns><see langword="true"/> when no empty cells remain; otherwise, <see langword="false"/>.</returns>
    public static bool IsFull(int[,] board)
    {
        for (var i = 0; i < Size; i++)
        {
            for (var j = 0; j < Size; j++)
            {
                if (board[i, j] == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Parses "0-8" or "row,col" into board coordinates.
    /// </summary>
    /// <param name="input">The user input to parse.</param>
    /// <param name="row">The parsed row index when the method returns <see langword="true"/>.</param>
    /// <param name="col">The parsed column index when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when the input represents a valid board cell; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseCell(string? input, out int row, out int col)
    {
        row = 0;
        col = 0;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var trimmed = input.Trim();
        if (trimmed.Contains(','))
        {
            var parts = trimmed.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out row) ||
                !int.TryParse(parts[1], out col))
            {
                return false;
            }
        }
        else
        {
            if (!int.TryParse(trimmed, out var pos) || pos < 0 || pos > 8)
            {
                return false;
            }

            row = pos / Size;
            col = pos % Size;
        }

        return row is >= 0 and < Size && col is >= 0 and < Size;
    }
}
