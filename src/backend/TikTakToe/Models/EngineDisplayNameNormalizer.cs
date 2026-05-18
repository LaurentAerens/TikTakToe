namespace TikTakToe.Models;

/// <summary>
/// Normalizes engine display names for case-insensitive and separator-insensitive comparisons.
/// </summary>
public static class EngineDisplayNameNormalizer
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var count = 0;
        foreach (var c in value)
        {
            if (char.IsWhiteSpace(c) || c == '_' || c == '-')
            {
                continue;
            }

            buffer[count++] = char.ToUpperInvariant(c);
        }

        return new string(buffer[..count]);
    }
}
