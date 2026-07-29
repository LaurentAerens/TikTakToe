namespace TikTakToe.Services;

using Microsoft.EntityFrameworkCore;
using Npgsql;
using TikTakToe.Data;
using TikTakToe.Models;

/// <summary>
/// Background worker that processes engine move jobs from the Postgres queue.
/// </summary>
public sealed class EngineMoveWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<EngineMoveWorker> logger) : BackgroundService
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PollDelay = TimeSpan.FromMilliseconds(500);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EngineMoveWorker starting.");

        // Perform startup recovery
        await PerformStartupRecoveryAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNextJobAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing engine move job.");
                await Task.Delay(PollDelay, stoppingToken);
            }
        }

        logger.LogInformation("EngineMoveWorker stopping.");
    }

    private async Task PerformStartupRecoveryAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Performing startup recovery...");

        using var scope = scopeFactory.CreateScope();
        var queue = scope.ServiceProvider.GetRequiredService<IEngineMoveQueue>();

        // Reclaim stale processing jobs
        var reclaimed = await queue.ReclaimStaleJobsAsync(cancellationToken);
        if (reclaimed > 0)
        {
            logger.LogInformation("Reclaimed {Count} stale jobs.", reclaimed);
        }

        // Find games that need jobs enqueued
        var gamesNeedingJobs = await queue.FindGamesNeedingJobsAsync(cancellationToken);
        if (gamesNeedingJobs.Length > 0)
        {
            logger.LogInformation("Found {Count} games needing engine jobs.", gamesNeedingJobs.Length);

            foreach (var gameId in gamesNeedingJobs)
            {
                await queue.TryEnqueueAsync(gameId, cancellationToken);
            }

            logger.LogInformation("Enqueued jobs for {Count} games.", gamesNeedingJobs.Length);
        }

        logger.LogInformation("Startup recovery complete.");
    }

    private async Task ProcessNextJobAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var queue = scope.ServiceProvider.GetRequiredService<IEngineMoveQueue>();

        // Claim a job using SKIP LOCKED
        var job = await ClaimJobAsync(dbContext, cancellationToken);
        if (job is null)
        {
            // No job available, wait before polling again
            await Task.Delay(PollDelay, cancellationToken);
            return;
        }

        logger.LogInformation("Processing job {JobId} for game {GameId} (attempt {Attempt})", job.Id, job.GameId, job.AttemptCount + 1);

        try
        {
            // Process the engine turn
            var game = await gameService.ApplyEngineTurnAsync(job.GameId, cancellationToken);

            // Mark job as completed
            job.Status = JobStatus.Completed;
            job.CompletedAtUtc = DateTime.UtcNow;
            job.LeaseOwner = null;
            job.LeaseExpiresAtUtc = null;

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Job {JobId} completed successfully.", job.Id);

            // Re-enqueue if the next player is still an engine (engine-vs-engine chain)
            if (game.WaitingForPlayerId.HasValue)
            {
                var waitingPlayer = game.Players.FirstOrDefault(p => p.Id == game.WaitingForPlayerId.Value);
                if (waitingPlayer is not null && waitingPlayer.IsEngine)
                {
                    logger.LogInformation("Next player is engine, enqueuing next job for game {GameId}", game.Id);
                    await queue.TryEnqueueAsync(game.Id, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Job {JobId} failed on attempt {Attempt}", job.Id, job.AttemptCount + 1);

            job.LastError = ex.Message.Length > 1000 ? ex.Message.Substring(0, 1000) : ex.Message;
            job.LeaseOwner = null;
            job.LeaseExpiresAtUtc = null;

            if (job.AttemptCount < job.MaxAttempts)
            {
                // Retry with exponential backoff
                job.AttemptCount++;
                job.Status = JobStatus.Pending;
                job.AvailableAtUtc = CalculateBackoff(job.AttemptCount);
                logger.LogInformation("Job {JobId} will retry at {AvailableAt} (attempt {Attempt})", job.Id, job.AvailableAtUtc, job.AttemptCount);
            }
            else
            {
                // Max attempts reached, mark as failed
                job.Status = JobStatus.Failed;
                job.CompletedAtUtc = DateTime.UtcNow;
                logger.LogError("Job {JobId} failed after {MaxAttempts} attempts", job.Id, job.MaxAttempts);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<EngineMoveJobModel?> ClaimJobAsync(GameDbContext dbContext, CancellationToken cancellationToken)
    {
        var leaseExpires = DateTime.UtcNow.Add(LeaseDuration);

        var sql = @"
            UPDATE engine_move_jobs
            SET status = 'Processing',
                attempt_count = attempt_count + 1,
                started_at_utc = NOW() AT TIME ZONE 'utc',
                lease_owner = @owner,
                lease_expires_at_utc = @leaseExpires
            WHERE id = (
                SELECT id FROM engine_move_jobs
                WHERE status = 'Pending'
                    AND available_at_utc <= NOW() AT TIME ZONE 'utc'
                ORDER BY created_at_utc
                FOR UPDATE SKIP LOCKED
                LIMIT 1
            )
            RETURNING id AS ""Id"";";

        var parameters = new[]
        {
            new Npgsql.NpgsqlParameter("owner", Environment.MachineName),
            new Npgsql.NpgsqlParameter("leaseExpires", leaseExpires),
        };

        var claimedRows = await dbContext.Database
            .SqlQueryRaw<ClaimedEngineMoveJobRow>(sql, parameters)
            .ToListAsync(cancellationToken);

        var claimedRow = claimedRows.FirstOrDefault();

        if (claimedRow is null)
        {
            return null;
        }

        // Load tracked entity by key for update operations.
        return await dbContext.EngineMoveJobs
            .SingleAsync(x => x.Id == claimedRow.Id, cancellationToken);
    }

    private sealed class ClaimedEngineMoveJobRow
    {
        public Guid Id { get; init; }
    }

    private static DateTime CalculateBackoff(int attemptCount)
    {
        // Exponential backoff: 2^attempt seconds, capped at 5 minutes
        var delaySeconds = Math.Min(Math.Pow(2, attemptCount), 300);
        return DateTime.UtcNow.AddSeconds(delaySeconds);
    }
}
