using TikTakToe.ConsoleApp;

Console.WriteLine("=== TikTakToe vs Engine ===\n");

while (true)
{
    Console.WriteLine("Choose mode:");
    Console.WriteLine("1) Play against engine");
    Console.WriteLine("2) Run tournament");
    var modeChoice = ConsoleIO.ReadLine("Enter choice (1-2, default 1): ");

    if (modeChoice?.Trim() == "2")
    {
        TournamentMode.Run();
    }
    else
    {
        PlayMode.Run();
    }

    if (!ConsoleIO.Confirm("\nPlay again?", defaultYes: false))
    {
        break;
    }

    Console.WriteLine();
}
