namespace TikTakToe.Services;

using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Models;

/// <summary>
/// Default implementation for enqueueing engine move jobs.
/// </summary>
public sealed class EngineMoveQueue(GameDbContext dbContext) : IEngineMoveQueue
{
    /// <inheritdoc />
    public async Task<bool> TryEnqueueAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        // Try to insert a new Pending job. The unique partial index on (game_id) where status = 'Pending'
        // will prevent duplicates, so we catch the exception and return false.
        try
        {
            var job = new EngineMoveJobModel
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Status = JobStatus.Pending,
                AttemptCount = 0,
                MaxAttempts = 5,
                CreatedAtUtc = DateTime.UtcNow,
                AvailableAtUtc = DateTime.UtcNow,
            };

            dbContext.EngineMoveJobs.Add(job);
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // A Pending job already exists for this game
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> ReclaimStaleJobsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var staleJobs = await dbContext.EngineMoveJobs
            .Where(j => j.Status == JobStatus.Processing
                && j.LeaseExpiresAtUtc.HasValue
                && j.LeaseExpiresAtUtc.Value < now)
            .ToListAsync(cancellationToken);

        foreach (var job in staleJobs)
        {
            job.Status = JobStatus.Pending;
            job.AvailableAtUtc = DateTime.UtcNow;
            job.LeaseOwner = null;
            job.LeaseExpiresAtUtc = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return staleJobs.Count;
    }

    /// <inheritdoc />
    public async Task<Guid[]> FindGamesNeedingJobsAsync(CancellationToken cancellationToken = default)
    {
        // Find games where:
        // 1. WaitingForPlayerId is set (game not over)
        // 2. The waiting player is an engine
        // 3. No Pending or Processing job exists for this game
        var gamesNeedingJobs = await dbContext.Games
            .Where(g => g.WaitingForPlayerId.HasValue
                && g.GamePlayers.Any(gp => gp.PlayerId == g.WaitingForPlayerId.Value && gp.Player.IsEngine)
                && !dbContext.EngineMoveJobs
                    .Any(j => j.GameId == g.Id && (j.Status == JobStatus.Pending || j.Status == JobStatus.Processing)))
            .Select(g => g.Id)
            .ToArrayAsync(cancellationToken);

        return gamesNeedingJobs;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // PostgreSQL unique constraint violation error code is 23505
        return ex.InnerException?.Message.Contains("23505") == true
            || ex.InnerException?.Message.Contains("unique constraint") == true
            || ex.InnerException?.Message.Contains("duplicate key") == true;
    }
}
