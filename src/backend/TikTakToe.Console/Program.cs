using TikTakToe.Engines;
using TikTakToe.Engines.Interface;
using TikTakToe.Engines.Exceptions;

Console.WriteLine("=== TikTakToe vs Engine ===\n");

while (true)
{
    // Select engine
    Console.WriteLine("Choose engine:");
    Console.WriteLine("1) Random");
    Console.WriteLine("2) Classical");
    Console.WriteLine("3) HalfDepth");
    Console.WriteLine("4) Oppertunity");
    Console.WriteLine("5) Halftunity");
    Console.Write("Enter choice (1-5, default 1): ");
    var engineChoice = Console.ReadLine();
    IEngine engine = engineChoice switch
    {
        "2" => new ClassicalEngine(),
        "3" => new HalfDepthEngine(),
        "4" => new OppertunityEngine(),
        "5" => new HalftunityEngine(),
        _ => new RandomEngine()
    };

    // Set depth for minimax-based engines (Classical/HalfDepth)
    int? searchDepth = null;
    if (engine is MinimaxEngineBase)
    {
        Console.Write("\nSet search depth (leave blank for full search): ");
        var depthInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(depthInput) && int.TryParse(depthInput, out var depth) && depth > 0)
        {
            searchDepth = depth;
            Console.WriteLine($"Search depth set to {searchDepth}");
        }
        else if (!string.IsNullOrWhiteSpace(depthInput))
        {
            Console.WriteLine("Invalid depth, using full search");
        }
    }

    // New fresh board for each game
    var board = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

    // Player selection
    Console.WriteLine("\nChoose your player:");
    Console.WriteLine("1) Player 1 (X)");
    Console.WriteLine("2) Player 2 (O)");
    Console.Write("Enter choice (1 or 2): ");
    var playerChoice = Console.ReadLine();
    var humanPlayer = playerChoice == "1" ? 1 : 2;
    var enginePlayer = humanPlayer == 1 ? 2 : 1;

    Console.WriteLine($"\nYou are Player {humanPlayer} ({(humanPlayer == 1 ? "X" : "O")})");
    Console.WriteLine($"Engine is Player {enginePlayer} ({(enginePlayer == 1 ? "X" : "O")})\n");

    var currentPlayer = 1;

    while (true)
    {
        PrintBoard(board);
        Console.WriteLine();

        if (currentPlayer == humanPlayer)
        {
            // Human's turn
            Console.WriteLine($"Your turn (Player {humanPlayer})");
            Console.WriteLine("Enter position (0-8) or row,col (e.g., '1,2'): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Invalid input. Try again.\n");
                continue;
            }

            int row, col;
            if (input.Contains(','))
            {
                var parts = input.Split(',');
                if (!int.TryParse(parts[0], out row) || !int.TryParse(parts[1], out col))
                {
                    Console.WriteLine("Invalid format. Use row,col (e.g., '1,2')\n");
                    continue;
                }
            }
            else
            {
                if (!int.TryParse(input, out var pos) || pos < 0 || pos > 8)
                {
                    Console.WriteLine("Invalid position. Use 0-8 or row,col\n");
                    continue;
                }
                row = pos / 3;
                col = pos % 3;
            }

            if (row < 0 || row > 2 || col < 0 || col > 2)
            {
                Console.WriteLine("Position out of bounds. Use row,col (0-2)\n");
                continue;
            }

            if (board[row, col] != 0)
            {
                Console.WriteLine("That square is already taken!\n");
                continue;
            }

            board[row, col] = humanPlayer;
            currentPlayer = enginePlayer;
        }
        else
        {
            // Engine's turn
            Console.WriteLine($"Engine's turn (Player {enginePlayer})");
            try
            {
                (board, var moveScore) = engine.Move(board, enginePlayer, searchDepth);
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

        if (CheckWin(board, humanPlayer))
        {
            PrintBoard(board);
            Console.WriteLine($"\n🎉 You won! Player {humanPlayer} has three in a row!");
            break;
        }
        else if (CheckWin(board, enginePlayer))
        {
            PrintBoard(board);
            Console.WriteLine($"\n😔 Engine won! Player {enginePlayer} has three in a row!");
            break;
        }
        else if (IsBoardFull(board))
        {
            PrintBoard(board);
            Console.WriteLine("\n🤝 It's a draw!");
            break;
        }
    }

    // Ask to play again
    Console.Write("\nPlay again? (y/n, default n): ");
    var again = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(again) || !again.Trim().ToLower().StartsWith("y"))
    {
        break;
    }
    Console.WriteLine();
}

bool CheckWin(int[,] b, int player)
{
    // Check rows
    for (var i = 0; i < 3; i++)
        if (b[i, 0] == player && b[i, 1] == player && b[i, 2] == player)
            return true;

    // Check columns
    for (var j = 0; j < 3; j++)
        if (b[0, j] == player && b[1, j] == player && b[2, j] == player)
            return true;

    // Check diagonals
    if (b[0, 0] == player && b[1, 1] == player && b[2, 2] == player) return true;
    if (b[0, 2] == player && b[1, 1] == player && b[2, 0] == player) return true;

    return false;
}

bool IsBoardFull(int[,] b)
{
    for (var i = 0; i < 3; i++)
        for (var j = 0; j < 3; j++)
            if (b[i, j] == 0)
                return false;
    return true;
}

void PrintBoard(int[,] b)
{
    Console.WriteLine("  0 1 2");
    for (var i = 0; i < 3; i++)
    {
        Console.Write($"{i} ");
        for (var j = 0; j < 3; j++)
        {
            var value = b[i, j] switch
            {
                0 => " ",
                1 => "X",
                2 => "O",
                _ => "?"
            };
            Console.Write($"{value} ");
        }
        Console.WriteLine();
    }
}
