namespace TikTakToe.ConsoleApp;

/// <summary>
/// Renders the tic-tac-toe board to the console.
/// </summary>
internal static class BoardPrinter
{
    /// <summary>
    /// Prints the current board state to the console.
    /// </summary>
    /// <param name="board">The board values to render.</param>
    public static void Print(int[,] board)
    {
        Console.WriteLine("  0 1 2");
        for (var i = 0; i < BoardRules.Size; i++)
        {
            Console.Write($"{i} ");
            for (var j = 0; j < BoardRules.Size; j++)
            {
                var value = board[i, j] switch
                {
                    0 => " ",
                    1 => "X",
                    2 => "O",
                    _ => "?",
                };
                Console.Write($"{value} ");
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Converts a player id to the corresponding display marker.
    /// </summary>
    /// <param name="player">The player identifier.</param>
    /// <returns>The marker to display for the player.</returns>
    public static char Mark(int player) => player == 1 ? 'X' : 'O';
}
