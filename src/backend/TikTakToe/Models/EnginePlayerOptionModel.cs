namespace TikTakToe.Models;

/// <summary>
/// Represents a specific engine player option (setting variant).
/// </summary>
public sealed record EnginePlayerOptionModel(Guid PlayerId, int? Depth);
