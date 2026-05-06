namespace TikTakToe.Services;

using Microsoft.EntityFrameworkCore;

using TikTakToe.Data;
using TikTakToe.Models;

/// <summary>
/// Default implementation for game persistence orchestration.
/// </summary>
public sealed class GameService(GameDbContext dbContext) : IGameService
{
    /// <inheritdoc />
    public async Task<GameModel> CreateAsync(int rows, int cols, IReadOnlyList<Guid> playerIds, CancellationToken cancellationToken = default)
    {
        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Board dimensions must be greater than zero.");
        }

        if (cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cols), "Board dimensions must be greater than zero.");
        }

        if (playerIds.Count == 0)
        {
            throw new ArgumentException("At least one player id is required.", nameof(playerIds));
        }

        var uniquePlayerIds = playerIds.Distinct().ToArray();
        if (uniquePlayerIds.Length != playerIds.Count)
        {
            throw new ArgumentException("Player ids must be unique.", nameof(playerIds));
        }

        var sourcePlayers = await dbContext.Players
            .AsNoTracking()
            .Where(x => uniquePlayerIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (sourcePlayers.Count != uniquePlayerIds.Length)
        {
            var found = sourcePlayers.Select(x => x.Id).ToHashSet();
            var missing = uniquePlayerIds.Where(x => !found.Contains(x)).ToArray();
            throw new ArgumentException($"Unknown player id(s): {string.Join(",", missing.Select(x => x.ToString("D")))}", nameof(playerIds));
        }

        var sourcePlayersById = sourcePlayers.ToDictionary(x => x.Id);
        var orderedSourcePlayers = uniquePlayerIds.Select(x => sourcePlayersById[x]).ToArray();

        var game = new GameModel
        {
            Board = new int[rows, cols],
            Players = orderedSourcePlayers
                .Select(player => new PlayerModel
                {
                    Id = Guid.NewGuid(),
                    IsEngine = player.IsEngine,
                    ExternalId = player.ExternalId,
                })
                .ToList(),
        };
        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync(cancellationToken);

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

        return game;
    }
}
