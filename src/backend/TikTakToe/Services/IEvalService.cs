namespace TikTakToe.Services;

/// <summary>
/// Coordinates board evaluation requests against a selected engine.
/// </summary>
public interface IEvalService
{
    /// <summary>
    /// Evaluates a board for the provided player from the selected engine.
    /// </summary>
    /// <param name="engineId">Engine capability identifier.</param>
    /// <param name="board">Board represented as a jagged matrix.</param>
    /// <param name="player">Current player value (1 or 2).</param>
    /// <param name="depth">Optional search depth. Null or 0 uses engine default behavior.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Evaluation score in the [-1000, 1000] range.</returns>
    Task<int> EvaluateAsync(Guid engineId, int[][]? board, int player, int? depth = null, CancellationToken cancellationToken = default);
}
