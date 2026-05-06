namespace TikTakToe.Services;

public sealed class EngineCapabilitySeederHostedService(
    IServiceProvider serviceProvider,
    ILogger<EngineCapabilitySeederHostedService> logger) : BackgroundService
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var engineLookupProvider = scope.ServiceProvider.GetRequiredService<IEngineLookupProvider>();
                await engineLookupProvider.EnsureCapabilitiesAsync(stoppingToken);
                logger.LogInformation("Engine capabilities ensured on startup.");
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                if (attempt == MaxAttempts)
                {
                    logger.LogError(ex, "Failed to ensure engine capabilities after {AttemptCount} attempts.", MaxAttempts);
                    return;
                }

                logger.LogWarning(
                    ex,
                    "Failed to ensure engine capabilities on attempt {Attempt} of {MaxAttempts}. Retrying in {RetryDelaySeconds} seconds.",
                    attempt,
                    MaxAttempts,
                    RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }
}
