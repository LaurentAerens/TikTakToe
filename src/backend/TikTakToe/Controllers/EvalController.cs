using TikTakToe.Models;
using TikTakToe.Services;

namespace TikTakToe.Controllers;

/// <summary>
/// Controller mappings for board evaluation operations.
/// </summary>
public static class EvalController
{
    /// <summary>
    /// Maps eval routes to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapEvalController(this WebApplication app)
    {
        app.MapPost("/eval", async (EvalRequest request, IEvalService evalService, CancellationToken cancellationToken) =>
        {
            try
            {
                var score = await evalService.EvaluateAsync(request.EngineId, request.Board, request.Player, request.Depth, cancellationToken);
                return Results.Ok(ApiResponse<EvalResponseDto>.Ok(new EvalResponseDto(score)));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<EvalResponseDto>.Fail(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<EvalResponseDto>.Fail(ex.Message));
            }
        })
        .WithName("EvalBoard")
        .WithSummary("Evaluates a board with the selected engine and player perspective");
    }

    private sealed record EvalRequest(Guid EngineId, int[][]? Board, int Player, int? Depth = null);
    private sealed record EvalResponseDto(int Score);
}
