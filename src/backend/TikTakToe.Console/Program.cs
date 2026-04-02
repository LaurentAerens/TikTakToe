using TikTakToe.Engines;
using TikTakToe.Engines.Interface;
using TikTakToe.Engines.Exceptions;

var engine = new RandomEngine();
var board = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

Console.WriteLine("=== TikTakToe vs Engine ===\n");

// Menu
Console.WriteLine("Choose your player:");
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
            (board, _) = engine.Move(board, enginePlayer);
            currentPlayer = humanPlayer;
        }
        catch (NoMoveAvailableException)
        {
            Console.WriteLine("Game Over: Board is full!");
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
