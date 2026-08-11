namespace TikTakToe.Services;

using TikTakToe.Models;

/// <summary>
/// Service interface for creating, retrieving, and seeding bot identities.
/// </summary>
public interface IBotService
{
    /// <summary>
    /// Creates a new bot identity with the specified engine capability and depth.
    /// </summary>
    Task<BotModel> CreateBotAsync(Guid engineCapabilityId, string? displayName = null, int? depth = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a bot by its primary identifier (which matches PlayerId).
    /// </summary>
    Task<BotModel?> GetBotByIdAsync(Guid botId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all registered bots (both default and custom-created).
    /// </summary>
    Task<IReadOnlyList<BotModel>> ListBotsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures default bot identities exist for all registered engine capabilities and depth options.
    /// </summary>
    Task EnsureDefaultBotsAsync(CancellationToken cancellationToken = default);
}
