namespace TikTakToe.Models;

/// <summary>
/// Represents an engine capability together with its backing engine player options.
/// </summary>
public sealed class EngineCapabilityWithPlayerModel : EngineCapabilityModel
{
    /// <summary>
    /// Gets or sets the backing engine player identifier.
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the list of engine player options supported by this engine.
    /// </summary>
    public IReadOnlyList<EnginePlayerOptionModel> PlayerOptions { get; set; } = [];
}
