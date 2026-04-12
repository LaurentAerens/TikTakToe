namespace TikTakToe.Models;

/// <summary>
/// Describes the persisted capabilities for an engine implementation.
/// </summary>
public sealed class EngineCapabilityModel
{
    /// <summary>
    /// Gets or sets the engine identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the human-readable engine name.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the maximum supported board size on the X-axis.
    /// </summary>
    public int MaxBoardSizeX { get; set; }

    /// <summary>
    /// Gets or sets the maximum supported board size on the Y-axis.
    /// </summary>
    public int MaxBoardSizeY { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether depth parameter is supported.
    /// </summary>
    public bool Depth { get; set; }
}