namespace TikTakToe.Services;

using TikTakToe.Models;

/// <summary>
/// Coordinates game persistence operations.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Creates a new game and initializes an empty board.
    /// </summary>
    /// <param name="rows">Board row count.</param>
    /// <param name="cols">Board column count.</param>
    /// <param name="playerIds">Source player identifiers to clone into the game.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created game model.</returns>
    Task<GameModel> CreateAsync(int rows, int cols, IReadOnlyList<Guid> playerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a game by identifier.
    /// </summary>
    /// <param name="gameId">Game identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The game model when found; otherwise null.</returns>
    Task<GameModel?> GetAsync(Guid gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Makes a move in a game, validating turns and running engines if necessary.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="playerId">The player identifier making the move.</param>
    /// <param name="x">The row index (null for engine moves).</param>
    /// <param name="y">The column index (null for engine moves).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated game model.</returns>
    Task<GameModel> MakeMoveAsync(Guid gameId, Guid playerId, int? x, int? y, CancellationToken cancellationToken = default);
}
