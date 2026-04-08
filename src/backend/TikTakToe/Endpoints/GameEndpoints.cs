using TikTakToe.Models;
using TikTakToe.Services;

namespace TikTakToe.Endpoints;

/// <summary>
/// Endpoints for game persistence operations.
/// </summary>
public static class GameEndpoints
{
    /// <summary>
    /// Maps game endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapGameEndpoints(this WebApplication app)
    {
        app.MapPost("/games", async (CreateGameRequest request, IGameService gameService, CancellationToken cancellationToken) =>
        {
            var rows = request.Rows <= 0 ? 3 : request.Rows;
            var cols = request.Cols <= 0 ? 3 : request.Cols;

            var game = await gameService.CreateAsync(rows, cols, cancellationToken);
            return Results.Created($"/games/{game.Id}", ApiResponse<GameDto>.Ok(ToDto(game)));
        })
        .WithName("CreateGame")
        .WithSummary("Creates a new game with a rectangular board stored as a PostgreSQL array");

        app.MapGet("/games/{id:guid}", async (Guid id, IGameService gameService, CancellationToken cancellationToken) =>
        {
            var game = await gameService.GetAsync(id, cancellationToken);
            if (game is null)
            {
                return Results.NotFound(ApiResponse<GameDto>.Fail("Game not found."));
            }

            return Results.Ok(ApiResponse<GameDto>.Ok(ToDto(game)));
        })
        .WithName("GetGame")
        .WithSummary("Returns a game by id");
    }

    private static GameDto ToDto(GameModel game)
    {
        return new GameDto(
            game.Id,
            ToJagged(game.Board),
            game.Players.Select(p => new PlayerDto(p.Id, p.IsEngine, p.ExternalId)).ToArray(),
            game.Moves.Select(m => new MoveDto(m.Id, m.X, m.Y, m.Value, m.MoveNumber)).ToArray());
    }

    private static int[][] ToJagged(int[,]? board)
    {
        if (board is null)
        {
            return [];
        }

        var rows = board.GetLength(0);
        var cols = board.GetLength(1);
        var result = new int[rows][];

        for (var row = 0; row < rows; row++)
        {
            result[row] = new int[cols];
            for (var col = 0; col < cols; col++)
            {
                result[row][col] = board[row, col];
            }
        }

        return result;
    }

    private sealed record CreateGameRequest(int Rows = 3, int Cols = 3);
    private sealed record GameDto(Guid Id, int[][] Board, PlayerDto[] Players, MoveDto[] Moves);
    private sealed record PlayerDto(long Id, bool IsEngine, string? ExternalId);
    private sealed record MoveDto(long Id, int X, int Y, int Value, int MoveNumber);
}