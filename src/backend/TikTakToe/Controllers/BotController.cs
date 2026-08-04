namespace TikTakToe.Controllers;

using TikTakToe.Models;
using TikTakToe.Services;

public static class BotController
{
    public static void MapBotController(this WebApplication app)
    {
        app.MapPost("/bots", async (CreateBotRequest request, IBotService botService, CancellationToken cancellationToken) =>
        {
            try
            {
                var bot = await botService.CreateBotAsync(
                    request.EngineCapabilityId,
                    request.DisplayName,
                    request.Depth,
                    cancellationToken);

                var dto = ToBotDto(bot);
                return Results.Created($"/bots/{bot.Id}", ApiResponse<BotDto>.Ok(dto));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<BotDto>.Fail(ex.Message));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ApiResponse<BotDto>.Fail(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<BotDto>.Fail(ex.Message));
            }
        })
        .WithName("CreateBot")
        .WithSummary("Create a bot with custom engine settings");

        app.MapGet("/bots", async (IBotService botService, CancellationToken cancellationToken) =>
        {
            var bots = await botService.ListBotsAsync(cancellationToken);
            var dtos = bots.Select(ToBotDto).ToArray();
            return Results.Ok(ApiResponse<BotDto[]>.Ok(dtos));
        })
        .WithName("ListBots")
        .WithSummary("List all registered bots");

        app.MapGet("/bots/{id:guid}", async (Guid id, IBotService botService, CancellationToken cancellationToken) =>
        {
            var bot = await botService.GetBotByIdAsync(id, cancellationToken);
            if (bot is null)
            {
                return Results.NotFound(ApiResponse<BotDto>.Fail($"Bot with ID '{id}' not found."));
            }

            return Results.Ok(ApiResponse<BotDto>.Ok(ToBotDto(bot)));
        })
        .WithName("GetBotById")
        .WithSummary("Get bot details by ID");
    }

    private static BotDto ToBotDto(BotModel bot)
    {
        var engineDisplayName = bot.EngineCapability?.DisplayName ?? string.Empty;
        return new BotDto(
            bot.Id,
            bot.Id,
            bot.EngineCapabilityId,
            engineDisplayName,
            bot.DisplayName,
            bot.Depth,
            bot.CreatedAtUtc);
    }

    public sealed record CreateBotRequest(Guid EngineCapabilityId, string? DisplayName, int? Depth);

    public sealed record BotDto(
        Guid Id,
        Guid PlayerId,
        Guid EngineCapabilityId,
        string EngineDisplayName,
        string DisplayName,
        int? Depth,
        DateTime CreatedAtUtc);
}
