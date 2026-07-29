namespace TikTakToe.Tests.Services;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Services;

public sealed class EngineMoveWorkerTests
{
    [Fact]
    public async Task PerformStartupRecoveryAsync_ReclaimsAndEnqueuesMissingJobs()
    {
        using var serviceProvider = BuildServiceProvider(
            out var queueMock,
            reclaimedCount: 2,
            gamesNeedingJobs: [Guid.NewGuid(), Guid.NewGuid()]);

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = Mock.Of<ILogger<EngineMoveWorker>>();
        var worker = new EngineMoveWorker(scopeFactory, logger);

        await InvokePrivateAsync(worker, "PerformStartupRecoveryAsync", CancellationToken.None);

        queueMock.Verify(x => x.ReclaimStaleJobsAsync(It.IsAny<CancellationToken>()), Times.Once);
        queueMock.Verify(x => x.FindGamesNeedingJobsAsync(It.IsAny<CancellationToken>()), Times.Once);
        queueMock.Verify(x => x.TryEnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleJobFailureAsync_WhenAttemptsRemain_SchedulesRetry()
    {
        await using var dbContext = CreateDbContext();
        var worker = new EngineMoveWorker(Mock.Of<IServiceScopeFactory>(), Mock.Of<ILogger<EngineMoveWorker>>());

        var job = new EngineMoveJobModel
        {
            Id = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            Status = JobStatus.Processing,
            AttemptCount = 1,
            MaxAttempts = 3,
            LeaseOwner = "worker-1",
            LeaseExpiresAtUtc = DateTime.UtcNow.AddMinutes(1),
        };

        dbContext.EngineMoveJobs.Add(job);
        await dbContext.SaveChangesAsync();

        var longMessage = new string('x', 1200);
        await InvokePrivateAsync(
            worker,
            "HandleJobFailureAsync",
            dbContext,
            job,
            new InvalidOperationException(longMessage),
            CancellationToken.None);

        Assert.Equal(JobStatus.Pending, job.Status);
        Assert.Equal(2, job.AttemptCount);
        Assert.Null(job.LeaseOwner);
        Assert.Null(job.LeaseExpiresAtUtc);
        Assert.NotNull(job.LastError);
        Assert.Equal(1000, job.LastError!.Length);
        Assert.Null(job.CompletedAtUtc);
        Assert.True(job.AvailableAtUtc > DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public async Task HandleJobFailureAsync_WhenMaxAttemptsReached_MarksJobFailed()
    {
        await using var dbContext = CreateDbContext();
        var worker = new EngineMoveWorker(Mock.Of<IServiceScopeFactory>(), Mock.Of<ILogger<EngineMoveWorker>>());

        var job = new EngineMoveJobModel
        {
            Id = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            Status = JobStatus.Processing,
            AttemptCount = 3,
            MaxAttempts = 3,
            LeaseOwner = "worker-2",
            LeaseExpiresAtUtc = DateTime.UtcNow.AddMinutes(1),
        };

        dbContext.EngineMoveJobs.Add(job);
        await dbContext.SaveChangesAsync();

        await InvokePrivateAsync(
            worker,
            "HandleJobFailureAsync",
            dbContext,
            job,
            new ArgumentException("boom"),
            CancellationToken.None);

        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.Equal(3, job.AttemptCount);
        Assert.NotNull(job.CompletedAtUtc);
        Assert.Null(job.LeaseOwner);
        Assert.Null(job.LeaseExpiresAtUtc);
    }

    [Fact]
    public void CalculateBackoff_WithLargeAttempt_CapsAtFiveMinutes()
    {
        var worker = new EngineMoveWorker(Mock.Of<IServiceScopeFactory>(), Mock.Of<ILogger<EngineMoveWorker>>());
        var now = DateTime.UtcNow;

        var next = (DateTime)InvokePrivate(worker, "CalculateBackoff", 20)!;

        Assert.True(next >= now.AddSeconds(295));
        Assert.True(next <= now.AddSeconds(305));
    }

    private static async Task InvokePrivateAsync(object target, string methodName, params object[] args)
    {
        var result = InvokePrivate(target, methodName, args);
        if (result is Task task)
        {
            await task;
            return;
        }

        throw new InvalidOperationException($"Method '{methodName}' did not return a Task.");
    }

    private static object? InvokePrivate(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Method '{methodName}' not found.");

        return method.Invoke(target, args);
    }

    private static ServiceProvider BuildServiceProvider(
        out Mock<IEngineMoveQueue> queueMock,
        int reclaimedCount,
        Guid[] gamesNeedingJobs)
    {
        queueMock = new Mock<IEngineMoveQueue>();
        queueMock.Setup(x => x.ReclaimStaleJobsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(reclaimedCount);
        queueMock.Setup(x => x.FindGamesNeedingJobsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(gamesNeedingJobs);
        queueMock.Setup(x => x.TryEnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var queue = queueMock.Object;

        var services = new ServiceCollection();
        services.AddScoped(_ => queue);

        return services.BuildServiceProvider();
    }

    private static GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GameDbContext(options);
    }
}
