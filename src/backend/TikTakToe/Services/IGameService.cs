using TikTakToe.Models;

namespace TikTakToe.Services;

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
}