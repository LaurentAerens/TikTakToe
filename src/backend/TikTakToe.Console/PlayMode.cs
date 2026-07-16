namespace TikTakToe.ConsoleApp;

using TikTakToe.Engines.Exceptions;
using TikTakToe.Tournament;

internal static class PlayMode
{
    /// <summary>
    /// Runs the interactive single-player console mode.
    /// </summary>
    public static void Run()
    {
        var engineInfo = EngineMenu.PromptSingleEngine();
        var engine = engineInfo.Factory();

        int? searchDepth = null;
        if (engineInfo.SupportsDepth)
        {
            searchDepth = ConsoleIO.ReadOptionalPositiveInt("\nSet search depth (leave blank for full search): ");
            if (searchDepth.HasValue)
            {
                Console.WriteLine($"Search depth set to {searchDepth}");
            }
        }

        var board = BoardRules.CreateEmpty();

        Console.WriteLine("\nChoose your player:");
        Console.WriteLine("1) Player 1 (X)");
        Console.WriteLine("2) Player 2 (O)");
        var humanPlayer = ConsoleIO.ReadInt("Enter choice (1 or 2, default 1): ", 1) == 2 ? 2 : 1;
        var enginePlayer = humanPlayer == 1 ? 2 : 1;

        Console.WriteLine($"\nYou are Player {humanPlayer} ({BoardPrinter.Mark(humanPlayer)})");
        Console.WriteLine($"Engine is {engineInfo.Name} as Player {enginePlayer} ({BoardPrinter.Mark(enginePlayer)})\n");

        var currentPlayer = 1;
        while (true)
        {
            BoardPrinter.Print(board);
            Console.WriteLine();

            if (currentPlayer == humanPlayer)
            {
                if (!TryHumanMove(board, humanPlayer))
                {
                    continue;
                }

                currentPlayer = enginePlayer;
            }
            else
            {
                Console.WriteLine($"Engine's turn (Player {enginePlayer})");
                try
                {
                    (board, var moveScore) = GameRunner.MakeMove(engine, board, enginePlayer, searchDepth);
                    Console.WriteLine($"Engine move score: {moveScore}");
                    currentPlayer = humanPlayer;
                }
                catch (NoMoveAvailableException)
                {
                    Console.WriteLine("Game Over: Board is full!");
                    break;
                }
                catch (BoardSizeNotSupportedException ex)
                {
                    Console.WriteLine($"Engine does not support this board size: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine();

            if (BoardRules.PlayerHasWon(board, humanPlayer))
            {
                BoardPrinter.Print(board);
                Console.WriteLine($"\nYou won! Player {humanPlayer} has three in a row!");
                break;
            }

            if (BoardRules.PlayerHasWon(board, enginePlayer))
            {
                BoardPrinter.Print(board);
                Console.WriteLine($"\nEngine won! Player {enginePlayer} has three in a row!");
                break;
            }

            if (BoardRules.IsFull(board))
            {
                BoardPrinter.Print(board);
                Console.WriteLine("\nIt's a draw!");
                break;
            }
        }
    }

    private static bool TryHumanMove(int[,] board, int humanPlayer)
    {
        Console.WriteLine($"Your turn (Player {humanPlayer})");
        var input = ConsoleIO.ReadLine("Enter position (0-8) or row,col (e.g. '1,2'): ");

        if (!BoardRules.TryParseCell(input, out var row, out var col))
        {
            Console.WriteLine("Invalid input. Use 0-8 or row,col (0-2).\n");
            return false;
        }

        if (board[row, col] != 0)
        {
            Console.WriteLine("That square is already taken!\n");
            return false;
        }

        board[row, col] = humanPlayer;
        return true;
    }
}
