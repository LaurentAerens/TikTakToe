namespace TikTakToe.ConsoleApp;

using TikTakToe.Tournament;

/// <summary>
/// Console menu built on top of <see cref="EngineRegistry"/> (single source of truth).
/// </summary>
internal static class EngineMenu
{
    /// <summary>
    /// Prints the available engines to the console.
    /// </summary>
    public static void PrintAvailableEngines()
    {
        Console.WriteLine("Available engines:");
        for (var i = 0; i < EngineRegistry.AllEngines.Length; i++)
        {
            var engine = EngineRegistry.AllEngines[i];
            var depthNote = engine.SupportsDepth ? "depth-capable" : "no depth";
            Console.WriteLine($"{i + 1}) {engine.Name} ({engine.Id}, {depthNote})");
        }
    }

    /// <summary>
    /// Prompts the user to choose a single engine from the menu.
    /// </summary>
    /// <param name="defaultMenuIndex">The default menu index to use when the input is empty.</param>
    /// <returns>The selected engine information.</returns>
    public static EngineRegistry.EngineInfo PromptSingleEngine(int defaultMenuIndex = 1)
    {
        PrintAvailableEngines();
        var max = EngineRegistry.AllEngines.Length;
        var choice = ConsoleIO.ReadInt($"Enter choice (1-{max}, default {defaultMenuIndex}): ", defaultMenuIndex);
        return EngineRegistry.GetByMenuIndex(choice) ?? EngineRegistry.AllEngines[defaultMenuIndex - 1];
    }

    /// <summary>
    /// Prompts the user to select engines for a tournament.
    /// </summary>
    /// <returns>The selected engine identifiers.</returns>
    public static List<string> PromptEngineIdsForTournament()
    {
        PrintAvailableEngines();
        Console.WriteLine();
        Console.WriteLine("Select engines to include in tournament:");
        Console.WriteLine("Enter engine numbers or ids separated by commas (e.g. '1,2,3' or 'classical,random'), or 'all':");

        var selection = Console.ReadLine();
        return EngineRegistry.ParseMenuSelection(selection);
    }
}
