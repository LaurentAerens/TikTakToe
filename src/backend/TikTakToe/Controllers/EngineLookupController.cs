using TikTakToe.Models;
using TikTakToe.Services;

namespace TikTakToe.Controllers;

public static class EngineLookupController
{
    public static void MapEngineLookupController(this WebApplication app)
    {
        app.MapGet("/engines", async (IEngineLookupProvider provider, CancellationToken cancellationToken) =>
        {
            await provider.EnsureCapabilitiesAsync(cancellationToken);
            var capabilities = await provider.ListCapabilitiesAsync(cancellationToken);
            var result = capabilities
                .Select(x => new EngineCapabilityDto(x.Id, x.PlayerId, x.DisplayName, x.MaxBoardSizeX, x.MaxBoardSizeY, x.Depth))
                .ToArray();

            return Results.Ok(ApiResponse<EngineCapabilityDto[]>.Ok(result));
        })
        .WithName("ListEngineCapabilities")
        .WithSummary("Lists all engines and their capabilities");

        app.MapGet("/engines/resolve-display-name/{id:guid}", async (Guid id, IEngineLookupProvider provider, CancellationToken cancellationToken) =>
        {
            await provider.EnsureCapabilitiesAsync(cancellationToken);
            var capability = await provider.GetByIdAsync(id, cancellationToken);
            if (capability is null)
            {
                return Results.NotFound(ApiResponse<EngineIdLookupDto>.Fail("Engine id not found."));
            }

            return Results.Ok(ApiResponse<EngineIdLookupDto>.Ok(new EngineIdLookupDto(capability.Id, capability.PlayerId, capability.DisplayName)));
        })
        .WithName("ResolveEngineDisplayName")
        .WithSummary("Converts engine id to display name");

        app.MapGet("/engines/resolve-engine-id/{playerId:guid}", async (Guid playerId, IEngineLookupProvider provider, CancellationToken cancellationToken) =>
        {
            await provider.EnsureCapabilitiesAsync(cancellationToken);
            var capability = await provider.GetByPlayerIdAsync(playerId, cancellationToken);
            if (capability is null)
            {
                return Results.NotFound(ApiResponse<EngineIdLookupDto>.Fail("Engine player id not found."));
            }

            return Results.Ok(ApiResponse<EngineIdLookupDto>.Ok(new EngineIdLookupDto(capability.Id, capability.PlayerId, capability.DisplayName)));
        })
        .WithName("ResolveEngineIdByPlayerId")
        .WithSummary("Converts engine player id to engine id");

        app.MapGet("/engines/resolve-id", async (string displayName, IEngineLookupProvider provider, CancellationToken cancellationToken) =>
        {
            await provider.EnsureCapabilitiesAsync(cancellationToken);
            var capability = await provider.GetByDisplayNameAsync(displayName, cancellationToken);
            if (capability is null)
            {
                return Results.NotFound(ApiResponse<EngineIdLookupDto>.Fail("Engine display name not found."));
            }

            return Results.Ok(ApiResponse<EngineIdLookupDto>.Ok(new EngineIdLookupDto(capability.Id, capability.PlayerId, capability.DisplayName)));
        })
        .WithName("ResolveEngineId")
        .WithSummary("Converts display name to engine id");
    }

    private sealed record EngineCapabilityDto(Guid Id, Guid PlayerId, string DisplayName, int MaxBoardSizeX, int MaxBoardSizeY, bool Depth);
    private sealed record EngineIdLookupDto(Guid Id, Guid PlayerId, string DisplayName);
}