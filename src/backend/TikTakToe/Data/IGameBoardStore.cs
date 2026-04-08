namespace TikTakToe.Data;

/// <summary>
/// Handles persistence of game boards as native PostgreSQL arrays.
/// </summary>
public interface IGameBoardStore
{
    /// <summary>
    /// Persists a board for a given game identifier.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="board">The board to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetBoardAsync(Guid gameId, int[,] board, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a board for a given game identifier.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded board or null when no board is available.</returns>
    Task<int[,]?> GetBoardAsync(Guid gameId, CancellationToken cancellationToken = default);
}