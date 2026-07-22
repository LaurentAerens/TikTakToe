namespace TikTakToe.Controllers;

using Microsoft.AspNetCore.Mvc;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Models.Dto;
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
        .WithName("StartGameSession")
        .WithSummary("Start a new game session");

        app.MapGet("/games/{id:guid}", async (Guid id, IGameService gameService, CancellationToken cancellationToken) =>
        {
            var game = await gameService.GetAsync(id, cancellationToken);
            if (game is null)
            {
                return Results.NotFound(ApiResponse<GameDto>.Fail("Game not found."));
            }

            return Results.Ok(ApiResponse<GameDto>.Ok(ToDto(game)));
        })
        .WithName("GetGameState")
        .WithSummary("Get the current game state");

        app.MapPost("/games/{id:guid}/moves", async (
            Guid id,
            MakeMoveRequest request,
            [FromHeader(Name = "X-Player-Id")] string? xPlayerIdHeader,
            [FromHeader(Name = "Player-Id")] string? playerIdHeader,
            IGameService gameService,
            CancellationToken cancellationToken) =>
        {
            var headerValue = xPlayerIdHeader ?? playerIdHeader;
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return Results.BadRequest(ApiResponse<GameDto>.Fail("Player identification header (X-Player-Id or Player-Id) is missing."));
            }

            if (!Guid.TryParse(headerValue, out var playerId))
            {
                return Results.BadRequest(ApiResponse<GameDto>.Fail("Player identification header is not a valid GUID."));
            }

            try
            {
                var game = await gameService.MakeMoveAsync(id, playerId, request.X, request.Y, cancellationToken);
                return Results.Ok(ApiResponse<GameDto>.Ok(ToDto(game)));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<GameDto>.Fail(ex.Message));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ApiResponse<GameDto>.Fail(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<GameDto>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<GameDto>.Fail(ex.Message));
            }
        })
        .WithName("PlayTurn")
        .WithSummary("Play the next turn in a game");

        // Quick and ugly endpoint to create a player for testing
        app.MapPost("/players", async (CreatePlayerRequest request, GameDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var player = new PlayerModel
            {
                IsEngine = request.IsEngine,
                ExternalId = request.ExternalId,
            };
            dbContext.Players.Add(player);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Created($"/players/{player.Id}", ApiResponse<Guid>.Ok(player.Id));
        })
        .WithName("RegisterPlayer")
        .WithSummary("Register a player identity (testing)");
    }

    private static GameDto ToDto(GameModel game)
    {
        return new GameDto(
            game.Id,
            ToJagged(game.Board),
            game.GamePlayers
                .OrderBy(x => x.TurnOrder)
                .Select(x => x.Player)
                .Select(p => new PlayerDto(p.Id, p.IsEngine, p.ExternalId))
                .ToArray(),
            game.Moves.Select(m => new MoveDto(m.Id, m.X, m.Y, m.Value, m.MoveNumber)).ToArray(),
            game.WaitingForPlayerId);
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
}
