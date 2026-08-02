namespace TikTakToe.Tests.Services;

using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Services;

public sealed class EngineMoveQueueTests
{
    [Fact]
    public async Task TryEnqueueAsync_WithNewGame_AddsPendingJobAndReturnsTrue()
    {
        await using var dbContext = CreateDbContext();
        var queue = new EngineMoveQueue(dbContext);
        var gameId = Guid.NewGuid();

        var enqueued = await queue.TryEnqueueAsync(gameId);

        Assert.True(enqueued);

        var job = await dbContext.EngineMoveJobs.SingleAsync();
        Assert.Equal(gameId, job.GameId);
        Assert.Equal(JobStatus.Pending, job.Status);
        Assert.Equal(0, job.AttemptCount);
        Assert.Equal(5, job.MaxAttempts);
    }

    [Fact]
    public async Task ReclaimStaleJobsAsync_ReclaimsOnlyExpiredProcessingJobs()
    {
        await using var dbContext = CreateDbContext();
        var queue = new EngineMoveQueue(dbContext);
        var now = DateTime.UtcNow;

        var staleJob = new EngineMoveJobModel
        {
            GameId = Guid.NewGuid(),
            Status = JobStatus.Processing,
            LeaseOwner = "worker-a",
            LeaseExpiresAtUtc = now.AddMinutes(-1),
        };

        var activeLeaseJob = new EngineMoveJobModel
        {
            GameId = Guid.NewGuid(),
            Status = JobStatus.Processing,
            LeaseOwner = "worker-b",
            LeaseExpiresAtUtc = now.AddMinutes(2),
        };

        var pendingJob = new EngineMoveJobModel
        {
            GameId = Guid.NewGuid(),
            Status = JobStatus.Pending,
            LeaseOwner = null,
            LeaseExpiresAtUtc = null,
        };

        dbContext.EngineMoveJobs.AddRange(staleJob, activeLeaseJob, pendingJob);
        await dbContext.SaveChangesAsync();

        var reclaimed = await queue.ReclaimStaleJobsAsync();

        Assert.Equal(1, reclaimed);

        var refreshedStale = await dbContext.EngineMoveJobs.SingleAsync(x => x.Id == staleJob.Id);
        Assert.Equal(JobStatus.Pending, refreshedStale.Status);
        Assert.Null(refreshedStale.LeaseOwner);
        Assert.Null(refreshedStale.LeaseExpiresAtUtc);

        var refreshedActive = await dbContext.EngineMoveJobs.SingleAsync(x => x.Id == activeLeaseJob.Id);
        Assert.Equal(JobStatus.Processing, refreshedActive.Status);
        Assert.NotNull(refreshedActive.LeaseOwner);

        var refreshedPending = await dbContext.EngineMoveJobs.SingleAsync(x => x.Id == pendingJob.Id);
        Assert.Equal(JobStatus.Pending, refreshedPending.Status);
    }

    [Fact]
    public async Task FindGamesNeedingJobsAsync_ReturnsOnlyEngineTurnGamesWithoutActiveJobs()
    {
        await using var dbContext = CreateDbContext();
        var queue = new EngineMoveQueue(dbContext);

        var enginePlayer = new PlayerModel
        {
            Id = Guid.NewGuid(),
            IsEngine = true,
            ExternalId = Guid.NewGuid().ToString(),
        };
        var humanPlayer = new PlayerModel { Id = Guid.NewGuid(), IsEngine = false };
        dbContext.Players.AddRange(enginePlayer, humanPlayer);

        var includedGame = CreateGameWaitingFor(enginePlayer);
        var pendingJobGame = CreateGameWaitingFor(enginePlayer);
        var processingJobGame = CreateGameWaitingFor(enginePlayer);
        var humanTurnGame = CreateGameWaitingFor(humanPlayer);
        var completedGame = CreateGameWaitingFor(null);

        dbContext.Games.AddRange(includedGame, pendingJobGame, processingJobGame, humanTurnGame, completedGame);

        dbContext.EngineMoveJobs.AddRange(
            new EngineMoveJobModel { GameId = pendingJobGame.Id, Status = JobStatus.Pending },
            new EngineMoveJobModel { GameId = processingJobGame.Id, Status = JobStatus.Processing });

        await dbContext.SaveChangesAsync();

        var gameIds = await queue.FindGamesNeedingJobsAsync();

        Assert.Contains(includedGame.Id, gameIds);
        Assert.DoesNotContain(pendingJobGame.Id, gameIds);
        Assert.DoesNotContain(processingJobGame.Id, gameIds);
        Assert.DoesNotContain(humanTurnGame.Id, gameIds);
        Assert.DoesNotContain(completedGame.Id, gameIds);
        Assert.Single(gameIds);
    }

    private static GameModel CreateGameWaitingFor(PlayerModel? waitingPlayer)
    {
        var game = new GameModel
        {
            Id = Guid.NewGuid(),
            Board = new int[3, 3],
            WaitingForPlayerId = waitingPlayer?.Id,
        };

        var humanPlayer = new PlayerModel { Id = Guid.NewGuid(), IsEngine = false };

        game.GamePlayers.Add(new GamePlayerModel
        {
            GameId = game.Id,
            PlayerId = humanPlayer.Id,
            TurnOrder = 0,
            Game = game,
            Player = humanPlayer,
        });

        if (waitingPlayer is not null)
        {
            game.GamePlayers.Add(new GamePlayerModel
            {
                GameId = game.Id,
                PlayerId = waitingPlayer.Id,
                TurnOrder = 1,
                Game = game,
                Player = waitingPlayer,
            });
        }

        return game;
    }

    private static GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GameDbContext(options);
    }
}
