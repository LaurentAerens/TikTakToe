namespace TikTakToe.Controllers;

using TikTakToe.Models;
using TikTakToe.Models.Dto;
using TikTakToe.Services;


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
        .WithName("EstimateBoardPosition")
        .WithSummary("Estimate board advantage for a player");
    }




}
