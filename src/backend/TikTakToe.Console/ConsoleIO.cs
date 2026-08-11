namespace TikTakToe.ConsoleApp;

/// <summary>
/// Small console prompt helpers shared by play and tournament modes.
/// </summary>
internal static class ConsoleIO
{
    public static string? ReadLine(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    public static int ReadInt(string prompt, int defaultValue)
    {
        var input = ReadLine(prompt);
        if (string.IsNullOrWhiteSpace(input))
        {
            return defaultValue;
        }

        return int.TryParse(input.Trim(), out var value) ? value : defaultValue;
    }

    public static int? ReadOptionalPositiveInt(string prompt)
    {
        var input = ReadLine(prompt);
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        if (int.TryParse(input.Trim(), out var value) && value > 0)
        {
            return value;
        }

        Console.WriteLine("Invalid value, using default.");
        return null;
    }

    public static bool Confirm(string prompt, bool defaultYes = false)
    {
        var suffix = defaultYes ? "Y/n" : "y/N";
        var input = ReadLine($"{prompt} ({suffix}): ");
        if (string.IsNullOrWhiteSpace(input))
        {
            return defaultYes;
        }

        return input.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase);
    }
}
