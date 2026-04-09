using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Services;

namespace TikTakToe.Tests.Services;

public sealed class GameServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidDimensions_PersistsGameAndBoard()
    {
        await using var dbContext = CreateDbContext();
        var boardStore = new FakeBoardStore();
        var service = new GameService(dbContext, boardStore);

        var game = await service.CreateAsync(3, 4);

        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.NotNull(game.Board);
        Assert.Equal(3, game.Board!.GetLength(0));
        Assert.Equal(4, game.Board.GetLength(1));

        var persistedGame = await dbContext.Games.SingleAsync(x => x.Id == game.Id);
        Assert.Equal(game.Id, persistedGame.Id);

        var storedBoard = await boardStore.GetBoardAsync(game.Id);
        Assert.NotNull(storedBoard);
        Assert.Equal(3, storedBoard!.GetLength(0));
        Assert.Equal(4, storedBoard.GetLength(1));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidDimensions_ThrowsArgumentOutOfRangeException()
    {
        await using var dbContext = CreateDbContext();
        var boardStore = new FakeBoardStore();
        var service = new GameService(dbContext, boardStore);

        var invalidRowsException = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CreateAsync(0, 3));
        Assert.Equal("rows", invalidRowsException.ParamName);

        var invalidColsException = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CreateAsync(3, 0));
        Assert.Equal("cols", invalidColsException.ParamName);

        Assert.Empty(dbContext.Games);
    }

    [Fact]
    public async Task GetAsync_WhenGameExists_ReturnsGameWithBoard()
    {
        await using var dbContext = CreateDbContext();
        var boardStore = new FakeBoardStore();
        var service = new GameService(dbContext, boardStore);

        var createdGame = await service.CreateAsync(2, 2);
        var game = await service.GetAsync(createdGame.Id);

        Assert.NotNull(game);
        Assert.Equal(createdGame.Id, game!.Id);
        Assert.NotNull(game.Board);
        Assert.Equal(2, game.Board!.GetLength(0));
        Assert.Equal(2, game.Board.GetLength(1));
    }

    [Fact]
    public async Task GetAsync_WhenGameMissing_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var boardStore = new FakeBoardStore();
        var service = new GameService(dbContext, boardStore);

        var game = await service.GetAsync(Guid.NewGuid());

        Assert.Null(game);
    }

    private static GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GameDbContext(options);
    }

    private sealed class FakeBoardStore : IGameBoardStore
    {
        private readonly Dictionary<Guid, int[,]> boards = [];

        public Task SetBoardAsync(Guid gameId, int[,] board, CancellationToken cancellationToken = default)
        {
            boards[gameId] = board;
            return Task.CompletedTask;
        }

        public Task<int[,]?> GetBoardAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            boards.TryGetValue(gameId, out var board);
            return Task.FromResult<int[,]?>(board);
        }
    }
}