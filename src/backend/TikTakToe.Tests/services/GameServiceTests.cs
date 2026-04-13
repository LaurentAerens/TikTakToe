using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Services;

namespace TikTakToe.Tests.Services;

public sealed class GameServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidDimensions_PersistsGameAndBoard()
    {
        await using var dbContext = CreateDbContext();
        var service = new GameService(dbContext);
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

        var persistedPlayers = await dbContext.Players.Where(x => x.GameId == game.Id).ToListAsync();
        Assert.Equal(2, persistedPlayers.Count);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidDimensions_ThrowsArgumentOutOfRangeException()
    {
        await using var dbContext = CreateDbContext();
        var service = new GameService(dbContext);
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
        var service = new GameService(dbContext);
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
        var service = new GameService(dbContext);

        var game = await service.GetAsync(Guid.NewGuid());

        Assert.Null(game);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownPlayerId_ThrowsArgumentException()
    {
        await using var dbContext = CreateDbContext();
        var service = new GameService(dbContext);
        var playerIds = await SeedPlayersAsync(dbContext, 1);
        var unknownId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(3, 3, [playerIds[0], unknownId]));

        Assert.Equal("playerIds", ex.ParamName);
        Assert.Contains("Unknown player id", ex.Message, StringComparison.OrdinalIgnoreCase);
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