namespace TikTakToe.Models;

/// <summary>
/// Represents an engine capability together with its backing engine player id.
/// </summary>
public sealed record EngineCapabilityWithPlayerModel(
    Guid EngineId,
    Guid PlayerId,
    string DisplayName,
    int MaxBoardSizeX,
    int MaxBoardSizeY,
    bool Depth);