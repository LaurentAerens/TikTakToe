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
        new("Halftunity", 3, 3, true, () => new HalftunityEngine()),
        new("Oppertunity", 3, 3, true, () => new OppertunityEngine()),
        new("Random", 10000, 10000, false, () => new RandomEngine()),
    ];

    public async Task EnsureCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.EngineCapabilities
            .ToListAsync(cancellationToken);

        var existingByDisplayName = new Dictionary<string, EngineCapabilityModel>(StringComparer.OrdinalIgnoreCase);
        foreach (var capability in existing)
        {
            if (!existingByDisplayName.ContainsKey(capability.DisplayName))
            {
                existingByDisplayName[capability.DisplayName] = capability;
            }
        }

        var hasChanges = false;
        foreach (var registration in Registrations)
        {
            if (existingByDisplayName.TryGetValue(registration.DisplayName, out var capability))
            {
                if (capability.MaxBoardSizeX != registration.MaxBoardSizeX
                    || capability.MaxBoardSizeY != registration.MaxBoardSizeY
                    || capability.Depth != registration.Depth)
                {
                    capability.MaxBoardSizeX = registration.MaxBoardSizeX;
                    capability.MaxBoardSizeY = registration.MaxBoardSizeY;
                    capability.Depth = registration.Depth;
                    hasChanges = true;
                }

                continue;
            }

            dbContext.EngineCapabilities.Add(new EngineCapabilityModel
            {
                Id = Guid.NewGuid(),
                DisplayName = registration.DisplayName,
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
    }

    public async Task<IReadOnlyList<EngineCapabilityModel>> ListCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.EngineCapabilities
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<EngineCapabilityModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.EngineCapabilities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<EngineCapabilityModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
    {
        return await dbContext.EngineCapabilities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.DisplayName == displayName, cancellationToken);
    }

    public async Task<IEngine?> CreateEngineByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var capability = await dbContext.EngineCapabilities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (capability is null)
        {
            return null;
        }

        var registration = Registrations
            .SingleOrDefault(x => string.Equals(x.DisplayName, capability.DisplayName, StringComparison.OrdinalIgnoreCase));

        return registration is null ? null : registration.Factory();
    }

    private sealed record EngineRegistration(
        string DisplayName,
        int MaxBoardSizeX,
        int MaxBoardSizeY,
        bool Depth,
        Func<IEngine> Factory);
}