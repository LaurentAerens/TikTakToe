namespace TikTakToe.Tests.Services;

using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Services;

public sealed class GameServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidDimensions_PersistsGameAndBoard()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var playerIds = await SeedPlayersAsync(dbContext, 2);

        var game = await service.CreateAsync(3, 4, playerIds);

        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.NotNull(game.Board);
        Assert.Equal(3, game.Board!.GetLength(0));
        Assert.Equal(4, game.Board.GetLength(1));

        var persistedGame = await dbContext.Games.SingleAsync(x => x.Id == game.Id);
        Assert.Equal(game.Id, persistedGame.Id);
        Assert.NotNull(persistedGame.Board);
        Assert.Equal(3, persistedGame.Board!.GetLength(0));
        Assert.Equal(4, persistedGame.Board.GetLength(1));

        var persistedPlayers = await dbContext.Players
            .Where(x => playerIds.Contains(x.Id))
            .ToListAsync();
        Assert.Equal(2, persistedPlayers.Count);

        var persistedParticipants = await dbContext.Set<GamePlayerModel>()
            .Where(x => x.GameId == game.Id)
            .ToListAsync();
        Assert.Equal(2, persistedParticipants.Count);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidDimensions_ThrowsArgumentOutOfRangeException()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var playerIds = await SeedPlayersAsync(dbContext, 2);

        var invalidRowsException = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CreateAsync(0, 3, playerIds));
        Assert.Equal("rows", invalidRowsException.ParamName);

        var invalidColsException = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CreateAsync(3, 0, playerIds));
        Assert.Equal("cols", invalidColsException.ParamName);

        Assert.Empty(dbContext.Games);
    }

    [Fact]
    public async Task GetAsync_WhenGameExists_ReturnsGameWithBoard()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var playerIds = await SeedPlayersAsync(dbContext, 2);

        var createdGame = await service.CreateAsync(2, 2, playerIds);
        var game = await service.GetAsync(createdGame.Id);

        Assert.NotNull(game);
        Assert.Equal(createdGame.Id, game!.Id);
        Assert.NotNull(game.Board);
        Assert.Equal(2, game.Board!.GetLength(0));
        Assert.Equal(2, game.Board.GetLength(1));
        Assert.Equal(2, game.Players.Count);
    }

    [Fact]
    public async Task GetAsync_WhenGameMissing_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);

        var game = await service.GetAsync(Guid.NewGuid());

        Assert.Null(game);
    }

    [Fact]
    public async Task CreateAsync_WithSameSourcePlayerAcrossGames_CreatesSeparateInGameParticipants()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var sourcePlayerId = (await SeedPlayersAsync(dbContext, 1))[0];

        var firstGame = await service.CreateAsync(3, 3, [sourcePlayerId]);
        var secondGame = await service.CreateAsync(3, 3, [sourcePlayerId]);

        Assert.Single(firstGame.Players);
        Assert.Single(secondGame.Players);
        Assert.Equal(sourcePlayerId, firstGame.Players[0].Id);
        Assert.Equal(sourcePlayerId, secondGame.Players[0].Id);

        var firstParticipants = await dbContext.Set<GamePlayerModel>()
            .Where(x => x.GameId == firstGame.Id)
            .ToListAsync();
        var secondParticipants = await dbContext.Set<GamePlayerModel>()
            .Where(x => x.GameId == secondGame.Id)
            .ToListAsync();

        Assert.Single(firstParticipants);
        Assert.Single(secondParticipants);
        Assert.Equal(sourcePlayerId, firstParticipants[0].PlayerId);
        Assert.Equal(sourcePlayerId, secondParticipants[0].PlayerId);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownPlayerId_ThrowsArgumentException()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var playerIds = await SeedPlayersAsync(dbContext, 1);
        var unknownId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(3, 3, [playerIds[0], unknownId]));

        Assert.Equal("playerIds", ex.ParamName);
        Assert.Contains("Unknown player id", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MakeMoveAsync_WithValidHumanMove_AppliesAndRecordsMove()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var playerIds = await SeedPlayersAsync(dbContext, 2);

        var game = await service.CreateAsync(3, 3, playerIds);
        var player1Id = game.Players[0].Id;

        var updatedGame = await service.MakeMoveAsync(game.Id, player1Id, 1, 1);

        Assert.Equal(1, updatedGame.Board![1, 1]);
        Assert.Single(updatedGame.Moves);
        Assert.Equal(1, updatedGame.Moves[0].X);
        Assert.Equal(1, updatedGame.Moves[0].Y);
        Assert.Equal(1, updatedGame.Moves[0].Value);
        Assert.Equal(1, updatedGame.Moves[0].MoveNumber);
        Assert.Equal(game.Players[1].Id, updatedGame.WaitingForPlayerId);
    }

    [Fact]
    public async Task MakeMoveAsync_WithValidEngineMove_AppliesAndRecordsMove()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var service = new GameService(dbContext, provider);
        var capabilities = await provider.ListCapabilitiesAsync();
        var enginePlayerId = capabilities.First(x => x.DisplayName == "Random").PlayerId;
        var humanPlayerIds = await SeedPlayersAsync(dbContext, 1);

        // Game with Human player 1 (starts) and Random Engine player 2
        var game = await service.CreateAsync(3, 3, [humanPlayerIds[0], enginePlayerId]);
        var humanInGameId = game.Players[0].Id;
        var engineInGameId = game.Players[1].Id;

        // Player 1 (human) moves first
        game = await service.MakeMoveAsync(game.Id, humanInGameId, 0, 0);

        // Player 2 (engine) turn
        var updatedGame = await service.MakeMoveAsync(game.Id, engineInGameId, null, null);

        Assert.Equal(2, updatedGame.Moves.Count);
        var engineMove = updatedGame.Moves[1];
        Assert.Equal(2, engineMove.Value);
        Assert.Equal(2, engineMove.MoveNumber);
        Assert.Equal(2, updatedGame.Board![engineMove.X, engineMove.Y]);
        Assert.Equal(humanInGameId, updatedGame.WaitingForPlayerId);
    }

    [Fact]
    public async Task MakeMoveAsync_OutOfTurn_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var playerIds = await SeedPlayersAsync(dbContext, 2);

        var game = await service.CreateAsync(3, 3, playerIds);
        var player2Id = game.Players[1].Id; // Not player 2's turn (player 1 starts)

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.MakeMoveAsync(game.Id, player2Id, 1, 1));
    }

    [Fact]
    public async Task MakeMoveAsync_CellOccupied_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var playerIds = await SeedPlayersAsync(dbContext, 2);

        var game = await service.CreateAsync(3, 3, playerIds);
        var player1Id = game.Players[0].Id;
        var player2Id = game.Players[1].Id;

        game = await service.MakeMoveAsync(game.Id, player1Id, 1, 1);

        // Player 2 tries to play on the same occupied cell (1, 1)
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.MakeMoveAsync(game.Id, player2Id, 1, 1));
    }

    [Fact]
    public async Task MakeMoveAsync_GameOver_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        var service = new GameService(dbContext, provider);
        var playerIds = await SeedPlayersAsync(dbContext, 2);

        var game = await service.CreateAsync(3, 3, playerIds);
        var player1Id = game.Players[0].Id;
        var player2Id = game.Players[1].Id;

        // Play out a game where Player 1 wins
        // P1: (0,0) (0,1) (0,2)
        // P2: (1,0) (1,1)
        game = await service.MakeMoveAsync(game.Id, player1Id, 0, 0);
        game = await service.MakeMoveAsync(game.Id, player2Id, 1, 0);
        game = await service.MakeMoveAsync(game.Id, player1Id, 0, 1);
        game = await service.MakeMoveAsync(game.Id, player2Id, 1, 1);
        game = await service.MakeMoveAsync(game.Id, player1Id, 0, 2);

        Assert.True(GameRules.IsGameOver(game.Board));

        // Attempting to move after game is over should throw
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.MakeMoveAsync(game.Id, player2Id, 2, 2));
    }

    [Fact]
    public void GameRules_DetectsWinsAndDraws()
    {
        // 1. Horizontal win for player 1
        var board1 = new int[,]
        {
            { 1, 1, 1 },
            { 0, 2, 0 },
            { 0, 0, 2 },
        };
        Assert.Equal(1, GameRules.GetWinner(board1));
        Assert.True(GameRules.IsGameOver(board1));

        // 2. Vertical win for player 2
        var board2 = new int[,]
        {
            { 1, 2, 0 },
            { 1, 2, 0 },
            { 0, 2, 1 },
        };
        Assert.Equal(2, GameRules.GetWinner(board2));
        Assert.True(GameRules.IsGameOver(board2));

        // 3. Diagonal win
        var board3 = new int[,]
        {
            { 1, 0, 2 },
            { 0, 1, 2 },
            { 0, 0, 1 },
        };
        Assert.Equal(1, GameRules.GetWinner(board3));

        // 4. Draw game
        var board4 = new int[,]
        {
            { 1, 2, 1 },
            { 1, 2, 2 },
            { 2, 1, 1 },
        };
        Assert.Null(GameRules.GetWinner(board4));
        Assert.True(GameRules.IsBoardFull(board4));
        Assert.True(GameRules.IsGameOver(board4));

        // 5. In-progress game
        var board5 = new int[,]
        {
            { 1, 0, 2 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };
        Assert.Null(GameRules.GetWinner(board5));
        Assert.False(GameRules.IsBoardFull(board5));
        Assert.False(GameRules.IsGameOver(board5));
    }

    private static GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GameDbContext(options);
    }

    private static async Task<Guid[]> SeedPlayersAsync(GameDbContext dbContext, int count)
    {
        var players = Enumerable.Range(0, count)
            .Select(_ => new PlayerModel
            {
                Id = Guid.NewGuid(),
                IsEngine = false,
                ExternalId = null,
            })
            .ToArray();

        dbContext.Players.AddRange(players);
        await dbContext.SaveChangesAsync();
        return players.Select(x => x.Id).ToArray();
    }
}
