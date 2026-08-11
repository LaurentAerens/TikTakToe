namespace TikTakToe.Services;

using Microsoft.EntityFrameworkCore;

using TikTakToe.Data;
using TikTakToe.Models;

/// <summary>
/// Service implementation for managing bot entities and seeding default bot identities.
/// </summary>
public sealed class BotService : IBotService
{
    private readonly GameDbContext _dbContext;

    public BotService(GameDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<BotModel> CreateBotAsync(
        Guid engineCapabilityId,
        string? displayName = null,
        int? depth = null,
        CancellationToken cancellationToken = default)
    {
        var capability = await this._dbContext.EngineCapabilities
            .SingleOrDefaultAsync(x => x.Id == engineCapabilityId, cancellationToken);

        if (capability is null)
        {
            throw new KeyNotFoundException($"Engine capability with ID '{engineCapabilityId}' not found.");
        }

        if (!capability.Depth && depth.HasValue)
        {
            throw new ArgumentException($"Engine '{capability.DisplayName}' does not support a depth setting.", nameof(depth));
        }

        if (capability.Depth)
        {
            if (!depth.HasValue)
            {
                throw new ArgumentException($"Engine '{capability.DisplayName}' requires a depth setting.", nameof(depth));
            }

            if (depth.Value < 1 || depth.Value > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be between 1 and 9 for this engine.");
            }
        }

        var playerId = Guid.NewGuid();
        var player = new PlayerModel
        {
            Id = playerId,
            IsEngine = true,
            ExternalId = playerId.ToString("D"),
        };

        var botDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? (depth.HasValue ? $"{capability.DisplayName} (Depth {depth.Value})" : capability.DisplayName)
            : displayName;

        var bot = new BotModel
        {
            Id = playerId,
            EngineCapabilityId = capability.Id,
            EngineCapability = capability,
            Player = player,
            DisplayName = botDisplayName,
            Depth = depth,
            CreatedAtUtc = DateTime.UtcNow,
        };

        this._dbContext.Players.Add(player);
        this._dbContext.Bots.Add(bot);
        await this._dbContext.SaveChangesAsync(cancellationToken);

        return bot;
    }

    /// <inheritdoc />
    public async Task<BotModel?> GetBotByIdAsync(Guid botId, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Bots
            .Include(x => x.EngineCapability)
            .Include(x => x.Player)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == botId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BotModel>> ListBotsAsync(CancellationToken cancellationToken = default)
    {
        var bots = await this._dbContext.Bots
            .Include(x => x.EngineCapability)
            .Include(x => x.Player)
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        return bots;
    }

    /// <inheritdoc />
    public async Task EnsureDefaultBotsAsync(CancellationToken cancellationToken = default)
    {
        var capabilities = await this._dbContext.EngineCapabilities
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var existingBots = await this._dbContext.Bots
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var existingKeySet = existingBots
            .Select(b => (b.EngineCapabilityId, b.Depth))
            .ToHashSet();

        var hasChanges = false;

        foreach (var capability in capabilities)
        {
            var supportedDepths = capability.Depth ? Enumerable.Range(1, 9).Cast<int?>().ToArray() : new int?[] { null };

            foreach (var depth in supportedDepths)
            {
                if (existingKeySet.Contains((capability.Id, depth)))
                {
                    continue;
                }

                var playerId = Guid.NewGuid();
                var player = new PlayerModel
                {
                    Id = playerId,
                    IsEngine = true,
                    ExternalId = playerId.ToString("D"),
                };

                var botDisplayName = depth.HasValue
                    ? $"{capability.DisplayName} (Depth {depth.Value})"
                    : capability.DisplayName;

                var bot = new BotModel
                {
                    Id = playerId,
                    EngineCapabilityId = capability.Id,
                    DisplayName = botDisplayName,
                    Depth = depth,
                    CreatedAtUtc = DateTime.UtcNow,
                };

                this._dbContext.Players.Add(player);
                this._dbContext.Bots.Add(bot);
                existingKeySet.Add((capability.Id, depth));
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
