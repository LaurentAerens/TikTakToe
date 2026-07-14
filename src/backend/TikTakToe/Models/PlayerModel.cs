namespace TikTakToe.Models;

/// <summary>
/// Represents a reusable player identity.
/// </summary>
public sealed class PlayerModel
{
    /// <summary>
    /// Gets or sets the primary identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets a value indicating whether this player is controlled by an engine.
    /// </summary>
    public bool IsEngine { get; set; }

    /// <summary>
    /// Gets or sets an external identity reference when available.
    /// </summary>
    public string? ExternalId { get; set; }

}
