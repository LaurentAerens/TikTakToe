namespace TikTakToe.Models;

/// <summary>
/// Represents a bot identity, associating an engine capability with specific settings (e.g. depth) and a backing player identity.
/// bot.Id matches player.Id (1:1 relationship with PlayerModel).
/// </summary>
public class BotModel
{
    /// <summary>
    /// Gets or sets the bot primary identifier, which is identical to the backing PlayerId.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the engine capability this bot is based on.
    /// </summary>
    public Guid EngineCapabilityId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the engine capability.
    /// </summary>
    public EngineCapabilityModel? EngineCapability { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the backing player entity.
    /// </summary>
    public PlayerModel? Player { get; set; }

    /// <summary>
    /// Gets or sets the human-readable display name for this bot persona.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the configured search depth limit for this bot, or null for default depth.
    /// </summary>
    public int? Depth { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
