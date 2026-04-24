using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Engines;
using TikTakToe.Engines.Interface;
using TikTakToe.Models;

namespace TikTakToe.Services;

public sealed class EngineLookupProvider(GameDbContext dbContext) : IEngineLookupProvider
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
        new("Random", 10000, 10000, false, () => new RandomEngine()),
    ];

    public async Task EnsureCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        ValidateUniqueRegistrationDisplayNames();

        var existing = await dbContext.EngineCapabilities
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

            dbContext.EngineCapabilities.Add(new EngineCapabilityModel
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
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureEnginePlayersAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EngineCapabilityWithPlayerModel>> ListCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var capabilities = await dbContext.EngineCapabilities
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        var playersByExternalId = await GetEnginePlayersByExternalIdAsync(cancellationToken);
        return capabilities
            .Select(capability => ToCapabilityWithPlayer(capability, playersByExternalId))
            .ToArray();
    }

    public async Task<EngineCapabilityWithPlayerModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var capability = await dbContext.EngineCapabilities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (capability is null)
        {
            return null;
        }

        var playersByExternalId = await GetEnginePlayersByExternalIdAsync(cancellationToken);
        return ToCapabilityWithPlayer(capability, playersByExternalId);
    }

    public async Task<EngineCapabilityWithPlayerModel?> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var player = await dbContext.Players
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == playerId && x.IsEngine, cancellationToken);

        if (player is null || !TryParseEngineExternalId(player.ExternalId, out var engineId))
        {
            return null;
        }

        var byEngineId = await GetByIdAsync(engineId, cancellationToken);
        if (byEngineId is null)
        {
            return null;
        }

        return new EngineCapabilityWithPlayerModel
        {
            Id = byEngineId.Id,
            PlayerId = player.Id,
            DisplayName = byEngineId.DisplayName,
            MaxBoardSizeX = byEngineId.MaxBoardSizeX,
            MaxBoardSizeY = byEngineId.MaxBoardSizeY,
            Depth = byEngineId.Depth,
        };
    }

    public async Task<EngineCapabilityWithPlayerModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
    {
        var normalizedDisplayName = EngineDisplayNameNormalizer.Normalize(displayName);
        var capability = await dbContext.EngineCapabilities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.NormalizedDisplayName == normalizedDisplayName, cancellationToken);

        if (capability is null)
        {
            return null;
        }

        var playersByExternalId = await GetEnginePlayersByExternalIdAsync(cancellationToken);
        return ToCapabilityWithPlayer(capability, playersByExternalId);
    }

    public async Task<IEngine?> CreateEngineByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var capability = await GetByIdAsync(id, cancellationToken);

        if (capability is null)
        {
            return null;
        }

        return CreateEngineFromCapability(capability);
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
        var capability = await GetByPlayerIdAsync(playerId, cancellationToken);
        if (capability is null)
        {
            return null;
        }

        return await CreateEngineByIdAsync(capability.Id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<int>> GetSupportedPlayersByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var capability = await GetByIdAsync(id, cancellationToken);
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

    private async Task EnsureEnginePlayersAsync(CancellationToken cancellationToken)
    {
        var capabilities = await dbContext.EngineCapabilities
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var existingEnginePlayers = await dbContext.Players
            .Where(x => x.IsEngine && x.GameId == null)
            .ToListAsync(cancellationToken);

        var hasChanges = false;
        var playersByEngineId = new Dictionary<Guid, PlayerModel>();
        foreach (var player in existingEnginePlayers)
        {
            if (!TryParseEngineExternalId(player.ExternalId, out var parsedEngineId))
            {
                continue;
            }

            if (!playersByEngineId.TryAdd(parsedEngineId, player))
            {
                throw new InvalidOperationException($"Multiple engine players map to engine id '{parsedEngineId:D}'.");
            }

            var normalizedExternalId = parsedEngineId.ToString();
            if (!string.Equals(player.ExternalId, normalizedExternalId, StringComparison.Ordinal))
            {
                player.ExternalId = normalizedExternalId;
                hasChanges = true;
            }
        }

        foreach (var capability in capabilities)
        {
            if (playersByEngineId.ContainsKey(capability.Id))
            {
                continue;
            }

            dbContext.Players.Add(new PlayerModel
            {
                Id = Guid.NewGuid(),
                IsEngine = true,
                ExternalId = capability.Id.ToString(),
            });
            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<Dictionary<string, PlayerModel>> GetEnginePlayersByExternalIdAsync(CancellationToken cancellationToken)
    {
        var enginePlayers = await dbContext.Players
            .AsNoTracking()
            .Where(x => x.IsEngine && x.GameId == null && x.ExternalId != null)
            .ToListAsync(cancellationToken);

        var playersByExternalId = new Dictionary<string, PlayerModel>(StringComparer.OrdinalIgnoreCase);
        foreach (var player in enginePlayers)
        {
            if (!TryParseEngineExternalId(player.ExternalId, out var engineId))
            {
                continue;
            }

            var key = engineId.ToString();
            if (!playersByExternalId.TryAdd(key, player))
            {
                throw new InvalidOperationException($"Multiple engine players map to engine id '{key}'.");
            }
        }

        return playersByExternalId;
    }

    private static EngineCapabilityWithPlayerModel ToCapabilityWithPlayer(
        EngineCapabilityModel capability,
        IReadOnlyDictionary<string, PlayerModel> playersByExternalId)
    {
        var key = capability.Id.ToString();
        if (!playersByExternalId.TryGetValue(key, out var player))
        {
            throw new InvalidOperationException($"Engine capability '{capability.DisplayName}' has no mapped engine player.");
        }

        return new EngineCapabilityWithPlayerModel
        {
            Id = capability.Id,
            PlayerId = player.Id,
            DisplayName = capability.DisplayName,
            MaxBoardSizeX = capability.MaxBoardSizeX,
            MaxBoardSizeY = capability.MaxBoardSizeY,
            Depth = capability.Depth,
        };
    }

    private static bool TryParseEngineExternalId(string? externalId, out Guid engineId)
    {
        return Guid.TryParse(externalId, out engineId);
    }
}