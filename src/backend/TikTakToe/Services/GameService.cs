namespace TikTakToe.Services;

using Microsoft.EntityFrameworkCore;

using TikTakToe.Data;
using TikTakToe.Engines.Interface;
using TikTakToe.Models;

/// <summary>
/// Default implementation for game persistence orchestration.
/// </summary>
public sealed class GameService(GameDbContext dbContext, IEngineLookupProvider engineLookupProvider) : IGameService
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
            .Where(x => uniquePlayerIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (sourcePlayers.Count != uniquePlayerIds.Length)
        {
            var found = sourcePlayers.Select(x => x.Id).ToHashSet();
            var missing = uniquePlayerIds.Where(x => !found.Contains(x)).ToArray();
            throw new ArgumentException($"Unknown player id(s): {string.Join(",", missing.Select(x => x.ToString("D")))}", nameof(playerIds));
        }

        var sourcePlayersById = sourcePlayers.ToDictionary(x => x.Id);
        var orderedSourcePlayers = uniquePlayerIds.Select(x => sourcePlayersById[x]).ToList();
        var gameParticipants = orderedSourcePlayers
            .Select((sourcePlayer, index) => new GamePlayerModel
            {
                PlayerId = sourcePlayer.Id,
                TurnOrder = index,
                Player = sourcePlayer,
            })
            .ToList();

        var game = new GameModel
        {
            Board = new int[rows, cols],
            GamePlayers = gameParticipants,
            WaitingForPlayerId = orderedSourcePlayers[0].Id,
        };
        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync(cancellationToken);

        game.GamePlayers = game.GamePlayers
            .OrderBy(x => x.TurnOrder)
            .ToList();

        return game;
    }

    /// <inheritdoc />
    public async Task<GameModel?> GetAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var game = await dbContext.Games
            .Include(x => x.GamePlayers)
            .ThenInclude(x => x.Player)
            .Include(x => x.Moves)
            .SingleOrDefaultAsync(x => x.Id == gameId, cancellationToken);

        if (game is null)
        {
            return null;
        }

        return game;
    }

    /// <inheritdoc />
    public async Task<GameModel> MakeMoveAsync(Guid gameId, Guid playerId, int? x, int? y, CancellationToken cancellationToken = default)
    {
        var game = await dbContext.Games
            .Include(x => x.GamePlayers)
            .ThenInclude(x => x.Player)
            .Include(x => x.Moves)
            .SingleOrDefaultAsync(g => g.Id == gameId, cancellationToken);

        var orderedPlayers = game?.GamePlayers
            .OrderBy(x => x.TurnOrder)
            .Select(x => x.Player)
            .ToList();

        if (game is null)
        {
            throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        }

        if (orderedPlayers is null || orderedPlayers.Count == 0)
        {
            throw new InvalidOperationException("Game has no registered players.");
        }

        var board = game.Board ?? throw new InvalidOperationException("Game board is unavailable.");

        // Determine if game is already over
        if (GameRules.IsGameOver(board))
        {
            throw new InvalidOperationException("Game is already over.");
        }

        // Whose turn is it?
        var nextPlayerIndex = game.Moves.Count % orderedPlayers.Count;
        var expectedPlayer = orderedPlayers[nextPlayerIndex];

        // Is the requesting player in the game?
        var isParticipant = orderedPlayers.Any(p => p.Id == playerId);
        if (!isParticipant)
        {
            throw new ArgumentException($"Player {playerId} is not a participant in this game.", nameof(playerId));
        }

        // Is it the requesting player's turn?
        if (expectedPlayer.Id != playerId)
        {
            throw new InvalidOperationException($"It is not player {playerId}'s turn.");
        }

        var rows = board.GetLength(0);
        var cols = board.GetLength(1);

        int moveX;
        int moveY;
        var playerValue = nextPlayerIndex + 1;

        if (expectedPlayer.IsEngine)
        {
            if (string.IsNullOrWhiteSpace(expectedPlayer.ExternalId) || !Guid.TryParse(expectedPlayer.ExternalId, out var engineId))
            {
                throw new InvalidOperationException($"Engine player {expectedPlayer.Id} does not have a valid engine ID in ExternalId.");
            }

            var engine = await engineLookupProvider.CreateEngineByIdAsync(engineId, cancellationToken);
            if (engine is null)
            {
                throw new InvalidOperationException($"Engine with ID {engineId} could not be instantiated.");
            }

            // Save copy of current board to compare and find the chosen coordinates
            var oldBoard = (int[,])board.Clone();

            // Run engine to determine the move
            var (newBoard, _) = engine.Move(oldBoard, playerValue);

            // Verify the new board is valid
            if (newBoard is null || newBoard.GetLength(0) != rows || newBoard.GetLength(1) != cols)
            {
                throw new InvalidOperationException("Engine returned an invalid board.");
            }

            // Find where the move was placed
            moveX = -1;
            moveY = -1;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    if (oldBoard[r, c] == 0 && newBoard[r, c] == playerValue)
                    {
                        moveX = r;
                        moveY = c;
                        break;
                    }
                }

                if (moveX != -1)
                {
                    break;
                }
            }

            if (moveX == -1 || moveY == -1)
            {
                throw new InvalidOperationException("Engine did not make a valid move.");
            }

            // Update game board state
            game.Board = newBoard;
        }
        else
        {
            // Human player move validation
            if (!x.HasValue || !y.HasValue)
            {
                throw new ArgumentException("Coordinates x and y are required for human player moves.");
            }

            moveX = x.Value;
            moveY = y.Value;

            if (moveX < 0 || moveX >= rows || moveY < 0 || moveY >= cols)
            {
                throw new ArgumentOutOfRangeException(null, "Coordinates are out of board boundaries.");
            }

            if (board[moveX, moveY] != 0)
            {
                throw new InvalidOperationException("Cell is already occupied.");
            }

            // Update game board state
            board[moveX, moveY] = playerValue;
        }

        // Record the move
        var move = new MoveModel
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            X = moveX,
            Y = moveY,
            Value = playerValue,
            MoveNumber = game.Moves.Count + 1,
            CreatedAtUtc = DateTime.UtcNow,
        };

        dbContext.Moves.Add(move);
        game.UpdatedAtUtc = DateTime.UtcNow;

        // Update whose turn it is (null when the game is now over)
        if (GameRules.IsGameOver(game.Board))
        {
            game.WaitingForPlayerId = null;
        }
        else
        {
            var nextIndex = move.MoveNumber % orderedPlayers.Count;
            game.WaitingForPlayerId = orderedPlayers[nextIndex].Id;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return game;
    }
}
