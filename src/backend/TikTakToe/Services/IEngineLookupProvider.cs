namespace TikTakToe.Services;

using TikTakToe.Engines.Interface;
using TikTakToe.Models;

public interface IEngineLookupProvider
{
    Task EnsureCapabilitiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EngineCapabilityWithPlayerModel>> ListCapabilitiesAsync(CancellationToken cancellationToken = default);

    Task<EngineCapabilityWithPlayerModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EngineCapabilityWithPlayerModel?> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);

    Task<EngineCapabilityWithPlayerModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default);

    Task<IEngine?> CreateEngineByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an engine instance from a pre-fetched capability, avoiding duplicate database lookups.
    /// </summary>
    /// <returns></returns>
    IEngine? CreateEngineFromCapability(EngineCapabilityWithPlayerModel capability);

    Task<IEngine?> CreateEngineByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> GetSupportedPlayersByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
