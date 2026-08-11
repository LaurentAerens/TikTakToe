namespace TikTakToe.Services;

using Microsoft.EntityFrameworkCore;

using TikTakToe.Data;
using TikTakToe.Engines;
using TikTakToe.Engines.Interface;
using TikTakToe.Models;

public sealed class EngineLookupProvider : IEngineLookupProvider
{
    private static readonly EngineRegistration[] Registrations =
    [
        new("Classical", 3, 3, true, () => new ClassicalEngine()),
        new("Half Depth", 3, 3, true, () => new HalfDepthEngine()),
        new("Inverse", 3, 3, true, () => new InverseEngine()),
        new("Disconnected", 3, 3, true, () => new DisconnectedEngine()),
        new("Halftunity", 3, 3, true, () => new HalftunityEngine()),
        new("Disconnicament", 3, 3, true, () => new DisconnicamentEngine()),
        new("Opportunity", 3, 3, true, () => new OpportunityEngine()),
        new("Predicament", 3, 3, true, () => new PredicamentEngine()),
        new("Sightline", 3, 3, true, () => new SightlineEngine()),
        new("Blindsight", 3, 3, true, () => new BlindsightEngine()),
        new("Random", 10000, 10000, false, () => new RandomEngine()),
    ];

    private readonly GameDbContext _dbContext;

    public EngineLookupProvider(GameDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task EnsureCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        ValidateUniqueRegistrationDisplayNames();

        var existing = await this._dbContext.EngineCapabilities
            .ToListAsync(cancellationToken);

        var existingByDisplayName = new Dictionary<string, EngineCapabilityModel>(StringComparer.Ordinal);
        foreach (var capability in existing)
        {
            var normalizedDisplayName = EngineDisplayNameNormalizer.Normalize(capability.DisplayName);
            if (!existingByDisplayName.TryAdd(normalizedDisplayName, capability))
            {
                throw new InvalidOperationException($"Multiple engine capabilities map to the same normalized display name '{normalizedDisplayName}'.");
            }
        }

        var hasChanges = false;
        foreach (var registration in Registrations)
        {
            var normalizedDisplayName = EngineDisplayNameNormalizer.Normalize(registration.DisplayName);
            if (existingByDisplayName.TryGetValue(normalizedDisplayName, out var capability))
            {
                if (capability.MaxBoardSizeX != registration.MaxBoardSizeX
                    || capability.MaxBoardSizeY != registration.MaxBoardSizeY
                    || capability.Depth != registration.Depth
                    || capability.NormalizedDisplayName != normalizedDisplayName)
                {
                    capability.MaxBoardSizeX = registration.MaxBoardSizeX;
                    capability.MaxBoardSizeY = registration.MaxBoardSizeY;
                    capability.Depth = registration.Depth;
                    capability.NormalizedDisplayName = normalizedDisplayName;
                    hasChanges = true;
                }

                continue;
            }

            this._dbContext.EngineCapabilities.Add(new EngineCapabilityModel
            {
                Id = Guid.NewGuid(),
                DisplayName = registration.DisplayName,
                NormalizedDisplayName = normalizedDisplayName,
                MaxBoardSizeX = registration.MaxBoardSizeX,
                MaxBoardSizeY = registration.MaxBoardSizeY,
                Depth = registration.Depth,
            });
            hasChanges = true;
        }

        if (hasChanges)
        {
            await this._dbContext.SaveChangesAsync(cancellationToken);
        }

        var botService = new BotService(this._dbContext);
        await botService.EnsureDefaultBotsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EngineCapabilityWithPlayerModel>> ListCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var capabilities = await this._dbContext.EngineCapabilities
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        var botsByEngineId = await this.GetBotsGroupedByEngineIdAsync(cancellationToken);
        return capabilities
            .Select(capability => ToCapabilityWithPlayer(capability, botsByEngineId))
            .ToArray();
    }

    public async Task<EngineCapabilityWithPlayerModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var capability = await this._dbContext.EngineCapabilities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (capability is null)
        {
            return null;
        }

        var botsByEngineId = await this.GetBotsGroupedByEngineIdAsync(cancellationToken);
        return ToCapabilityWithPlayer(capability, botsByEngineId);
    }

    public async Task<EngineCapabilityWithPlayerModel?> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var bot = await this._dbContext.Bots
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == playerId, cancellationToken);

        if (bot is not null)
        {
            var byEngineId = await this.GetByIdAsync(bot.EngineCapabilityId, cancellationToken);
            if (byEngineId is null)
            {
                return null;
            }

            return new EngineCapabilityWithPlayerModel
            {
                Id = byEngineId.Id,
                PlayerId = bot.Id,
                PlayerOptions = byEngineId.PlayerOptions,
                DisplayName = byEngineId.DisplayName,
                MaxBoardSizeX = byEngineId.MaxBoardSizeX,
                MaxBoardSizeY = byEngineId.MaxBoardSizeY,
                Depth = byEngineId.Depth,
            };
        }

        var player = await this._dbContext.Players
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == playerId && x.IsEngine, cancellationToken);

        if (player is null || !TryParseEngineExternalId(player.ExternalId, out var engineId, out _))
        {
            return null;
        }

        var fallbackByEngineId = await this.GetByIdAsync(engineId, cancellationToken);
        if (fallbackByEngineId is null)
        {
            return null;
        }

        return new EngineCapabilityWithPlayerModel
        {
            Id = fallbackByEngineId.Id,
            PlayerId = player.Id,
            PlayerOptions = fallbackByEngineId.PlayerOptions,
            DisplayName = fallbackByEngineId.DisplayName,
            MaxBoardSizeX = fallbackByEngineId.MaxBoardSizeX,
            MaxBoardSizeY = fallbackByEngineId.MaxBoardSizeY,
            Depth = fallbackByEngineId.Depth,
        };
    }

    public async Task<EngineCapabilityWithPlayerModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
    {
        var normalizedDisplayName = EngineDisplayNameNormalizer.Normalize(displayName);
        var capability = await this._dbContext.EngineCapabilities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.NormalizedDisplayName == normalizedDisplayName, cancellationToken);

        if (capability is null)
        {
            return null;
        }

        var botsByEngineId = await this.GetBotsGroupedByEngineIdAsync(cancellationToken);
        return ToCapabilityWithPlayer(capability, botsByEngineId);
    }

    public async Task<IEngine?> CreateEngineByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var capability = await this.GetByIdAsync(id, cancellationToken);

        if (capability is null)
        {
            return null;
        }

        return this.CreateEngineFromCapability(capability);
    }

    public IEngine? CreateEngineFromCapability(EngineCapabilityWithPlayerModel capability)
    {
        if (capability is null)
        {
            return null;
        }

        var registration = Registrations
            .SingleOrDefault(x => EngineDisplayNameNormalizer.Normalize(x.DisplayName) == EngineDisplayNameNormalizer.Normalize(capability.DisplayName));

        return registration is null ? null : registration.Factory();
    }

    public async Task<IEngine?> CreateEngineByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var resolution = await this.ResolveEnginePlayerAsync(playerId, cancellationToken);
        return resolution?.Engine;
    }

    public async Task<EnginePlayerResolution?> ResolveEnginePlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var bot = await this._dbContext.Bots
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == playerId, cancellationToken);

        if (bot is not null)
        {
            var engine = await this.CreateEngineByIdAsync(bot.EngineCapabilityId, cancellationToken);
            if (engine is not null)
            {
                return new EnginePlayerResolution(engine, bot.Depth);
            }
        }

        var player = await this._dbContext.Players
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == playerId && x.IsEngine, cancellationToken);

        if (player is null || !TryParseEngineExternalId(player.ExternalId, out var engineId, out var depth))
        {
            return null;
        }

        var fallbackEngine = await this.CreateEngineByIdAsync(engineId, cancellationToken);
        if (fallbackEngine is null)
        {
            return null;
        }

        return new EnginePlayerResolution(fallbackEngine, depth);
    }

    public async Task<IReadOnlyCollection<int>> GetSupportedPlayersByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var capability = await this.GetByIdAsync(id, cancellationToken);
        if (capability is null)
        {
            return [1, 2];
        }

        // All current engine implementations use the default IEngine.SupportedPlayers => [1, 2]
        // This method avoids the N+1 problem of instantiating each engine in the /engines endpoint.
        return [1, 2];
    }

    private static void ValidateUniqueRegistrationDisplayNames()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var registration in Registrations)
        {
            var normalizedDisplayName = EngineDisplayNameNormalizer.Normalize(registration.DisplayName);
            if (!seen.Add(normalizedDisplayName))
            {
                throw new InvalidOperationException($"Multiple registered engines map to the same normalized display name '{normalizedDisplayName}'.");
            }
        }
    }

    private static EngineCapabilityWithPlayerModel ToCapabilityWithPlayer(
        EngineCapabilityModel capability,
        IReadOnlyDictionary<Guid, List<BotModel>> botsByEngineId)
    {
        var playerOptions = new List<EnginePlayerOptionModel>();
        var primaryPlayerId = Guid.Empty;

        if (botsByEngineId.TryGetValue(capability.Id, out var bots))
        {
            foreach (var bot in bots)
            {
                playerOptions.Add(new EnginePlayerOptionModel(bot.Id, bot.Depth));
            }

            if (playerOptions.Count > 0)
            {
                primaryPlayerId = playerOptions[0].PlayerId;
            }
        }

        return new EngineCapabilityWithPlayerModel
        {
            Id = capability.Id,
            PlayerId = primaryPlayerId,
            PlayerOptions = playerOptions,
            DisplayName = capability.DisplayName,
            MaxBoardSizeX = capability.MaxBoardSizeX,
            MaxBoardSizeY = capability.MaxBoardSizeY,
            Depth = capability.Depth,
        };
    }

    private static bool TryParseEngineExternalId(string? externalId, out Guid engineId, out int? depth)
    {
        engineId = default;
        depth = null;

        if (string.IsNullOrWhiteSpace(externalId))
        {
            return false;
        }

        var parts = externalId.Split(':', 2);
        if (!Guid.TryParse(parts[0], out engineId))
        {
            return false;
        }

        if (parts.Length > 1 && parts[1].StartsWith("depth=", StringComparison.OrdinalIgnoreCase))
        {
            var depthStr = parts[1]["depth=".Length..];
            if (int.TryParse(depthStr, out var parsedDepth))
            {
                depth = parsedDepth;
            }
        }

        return true;
    }

    private async Task<Dictionary<Guid, List<BotModel>>> GetBotsGroupedByEngineIdAsync(CancellationToken cancellationToken)
    {
        var bots = await this._dbContext.Bots
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, List<BotModel>>();
        foreach (var bot in bots)
        {
            if (!result.TryGetValue(bot.EngineCapabilityId, out var list))
            {
                list = [];
                result[bot.EngineCapabilityId] = list;
            }

            list.Add(bot);
        }

        return result;
    }
}
