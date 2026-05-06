namespace TikTakToe.Controllers;

using TikTakToe.Models;
using TikTakToe.Services;

/// <summary>
/// Controller mappings for game persistence operations.
/// </summary>
public static class GameController
{
    private const int _maxBoardDimension = 10_000;
    private const int _minPlayers = 2;
    private const int _maxPlayers = 1000;

    /// <summary>
    /// Maps game controller routes to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapGameController(this WebApplication app)
    {
        app.MapPost("/games", async (CreateGameRequest request, IGameService gameService, CancellationToken cancellationToken) =>
        {
            var rows = request.Rows <= 0 ? 3 : request.Rows;
            var cols = request.Cols <= 0 ? 3 : request.Cols;
            var playerIds = request.PlayerIds;

            if (rows > _maxBoardDimension || cols > _maxBoardDimension)
            {
                return Results.BadRequest(
                    ApiResponse<GameDto>.Fail(
                        $"Board dimensions must be less than or equal to {_maxBoardDimension}. Requested rows={rows}, cols={cols}."));
            }

            if (playerIds is null || playerIds.Length < _minPlayers || playerIds.Length > _maxPlayers)
            {
                return Results.BadRequest(
                    ApiResponse<GameDto>.Fail($"A game requires between {_minPlayers} and {_maxPlayers} player ids."));
            }

            try
            {
                var game = await gameService.CreateAsync(rows, cols, playerIds, cancellationToken);
                return Results.Created($"/games/{game.Id}", ApiResponse<GameDto>.Ok(ToDto(game)));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<GameDto>.Fail(ex.Message));
            }
        })
        .WithName("CreateGame")
        .WithSummary("Creates a new game with a rectangular board stored as jsonb");

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

    private sealed record CreateGameRequest(int Rows = 3, int Cols = 3, Guid[]? PlayerIds = null);

    private sealed record GameDto(Guid Id, int[][] Board, PlayerDto[] Players, MoveDto[] Moves);

    private sealed record PlayerDto(Guid Id, bool IsEngine, string? ExternalId);

    private sealed record MoveDto(Guid Id, int X, int Y, int Value, int MoveNumber);
}
