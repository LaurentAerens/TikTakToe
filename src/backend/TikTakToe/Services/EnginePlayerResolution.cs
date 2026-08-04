namespace TikTakToe.Services;

using TikTakToe.Engines.Interface;

/// <summary>
/// Encapsulates a resolved engine instance along with its configured depth setting.
/// </summary>
public sealed record EnginePlayerResolution(IEngine Engine, int? Depth);
