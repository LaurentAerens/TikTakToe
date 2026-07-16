namespace TikTakToe.Tournament;

using TikTakToe.Engines;
using TikTakToe.Engines.Interface;

/// <summary>
/// Registry for all available engines with their display names and factory methods.
/// Designed to be portable between console and backend applications.
/// </summary>
public static class EngineRegistry
{
    public static readonly EngineInfo[] AllEngines =
    [
        new("random", "Random", static () => new RandomEngine(), false),
        new("classical", "Classical", static () => new ClassicalEngine(), true),
        new("halfdepth", "HalfDepth", static () => new HalfDepthEngine(), true),
        new("opportunity", "Opportunity", static () => new OpportunityEngine(), true),
        new("halftunity", "Halftunity", static () => new HalftunityEngine(), true),
        new("inverse", "Inverse", static () => new InverseEngine(), true),
        new("disconnected", "Disconnected", static () => new DisconnectedEngine(), true),
        new("predicament", "Predicament", static () => new PredicamentEngine(), true),
        new("disconnicament", "Disconnicament", static () => new DisconnicamentEngine(), true),
    ];

    public static IReadOnlyList<string> AllIds => AllEngines.Select(e => e.Id).ToArray();

    public static IEngine CreateEngine(string id)
    {
        var engine = GetEngineInfo(id);
        if (engine is null)
        {
            throw new ArgumentException($"Unknown engine id: {id}", nameof(id));
        }

        return engine.Factory();
    }

    public static EngineInfo? GetEngineInfo(string id)
    {
        return AllEngines.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Resolves a 1-based menu index (as shown in consoles/UIs) to engine info.
    /// </summary>
    /// <returns></returns>
    public static EngineInfo? GetByMenuIndex(int oneBasedIndex)
    {
        if (oneBasedIndex < 1 || oneBasedIndex > AllEngines.Length)
        {
            return null;
        }

        return AllEngines[oneBasedIndex - 1];
    }

    /// <summary>
    /// Parses a menu selection such as "1,3,5" or "all" into engine ids.
    /// Invalid tokens are ignored.
    /// </summary>
    /// <returns></returns>
    public static List<string> ParseMenuSelection(string? selection)
    {
        if (string.IsNullOrWhiteSpace(selection))
        {
            return [];
        }

        var trimmed = selection.Trim();
        if (trimmed.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return AllIds.ToList();
        }

        var ids = new List<string>();
        foreach (var token in trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(token, out var index))
            {
                var info = GetByMenuIndex(index);
                if (info is not null && !ids.Contains(info.Id, StringComparer.OrdinalIgnoreCase))
                {
                    ids.Add(info.Id);
                }
            }
            else
            {
                var info = GetEngineInfo(token);
                if (info is not null && !ids.Contains(info.Id, StringComparer.OrdinalIgnoreCase))
                {
                    ids.Add(info.Id);
                }
            }
        }

        return ids;
    }

    /// <summary>
    /// Maps a live engine instance back to its registry id when possible.
    /// Convention: ClassicalEngine -> "classical".
    /// </summary>
    /// <returns></returns>
    public static string GetId(IEngine engine)
    {
        var candidate = engine.GetType().Name
            .Replace("Engine", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return GetEngineInfo(candidate)?.Id ?? candidate;
    }

    public sealed record EngineInfo(
        string Id,
        string Name,
        Func<IEngine> Factory,
        bool SupportsDepth);
}
