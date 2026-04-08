using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Models;

namespace TikTakToe.Services;

/// <summary>
/// Default implementation for game persistence orchestration.
/// </summary>
public sealed class GameService(GameDbContext dbContext, IGameBoardStore gameBoardStore) : IGameService
{
    /// <inheritdoc />
    public async Task<GameModel> CreateAsync(int rows, int cols, CancellationToken cancellationToken = default)
    {
        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Board dimensions must be greater than zero.");
        }

        if (cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cols), "Board dimensions must be greater than zero.");
        }
        var game = new GameModel();
        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync(cancellationToken);

        var board = new int[rows, cols];

        try
        {
            await gameBoardStore.SetBoardAsync(game.Id, board, cancellationToken);
        }
        catch
        {
            dbContext.Games.Remove(game);
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
        game.Board = board;

        return game;
    }

    /// <inheritdoc />
    public async Task<GameModel?> GetAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var game = await dbContext.Games
            .Include(x => x.Players)
            .Include(x => x.Moves)
            .SingleOrDefaultAsync(x => x.Id == gameId, cancellationToken);

        if (game is null)
        {
            return null;
        }

        game.Board = await gameBoardStore.GetBoardAsync(game.Id, cancellationToken);
        return game;
    }
}