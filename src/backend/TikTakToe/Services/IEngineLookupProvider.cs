using TikTakToe.Engines.Interface;
using TikTakToe.Models;

namespace TikTakToe.Services;

public interface IEngineLookupProvider
{
    Task EnsureCapabilitiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EngineCapabilityModel>> ListCapabilitiesAsync(CancellationToken cancellationToken = default);

    Task<EngineCapabilityModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EngineCapabilityModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default);

    Task<IEngine?> CreateEngineByIdAsync(Guid id, CancellationToken cancellationToken = default);
}