namespace TikTakToe.Models;

/// <summary>
/// Represents an engine capability together with its backing engine player id.
/// </summary>
public sealed class EngineCapabilityWithPlayerModel : EngineCapabilityModel
{
    /// <summary>
    /// Gets or sets the backing engine player identifier.
    /// </summary>
    public Guid PlayerId { get; set; }
}
