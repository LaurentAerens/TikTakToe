namespace TikTakToe.Services;

/// <summary>
/// Interface for enqueueing engine move jobs.
/// </summary>
public interface IEngineMoveQueue
{
    /// <summary>
    /// Attempts to enqueue an engine move job for the specified game.
    /// No-ops if a Pending job already exists for the game.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a new job was enqueued; false if a pending job already existed.</returns>
    Task<bool> TryEnqueueAsync(Guid gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reclaims stale processing jobs (expired leases) by setting them back to Pending.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of jobs reclaimed.</returns>
    Task<int> ReclaimStaleJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds games that are waiting for an engine player but have no pending/processing job.
    /// Used for startup recovery.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The game IDs that need jobs enqueued.</returns>
    Task<Guid[]> FindGamesNeedingJobsAsync(CancellationToken cancellationToken = default);
}
