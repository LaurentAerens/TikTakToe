using TikTakToe.Engines.Interface;
using TikTakToe.Models;

namespace TikTakToe.Services;

public interface IEngineLookupProvider
{
    Task EnsureCapabilitiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EngineCapabilityWithPlayerModel>> ListCapabilitiesAsync(CancellationToken cancellationToken = default);

    Task<EngineCapabilityWithPlayerModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EngineCapabilityWithPlayerModel?> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);

    Task<EngineCapabilityWithPlayerModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default);

    Task<IEngine?> CreateEngineByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEngine?> CreateEngineByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);
}