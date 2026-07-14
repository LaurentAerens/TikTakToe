namespace TikTakToe.Services;

using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;

/// <summary>
/// Ensures the database is available and optionally reset before the application starts.
/// </summary>
public sealed class DatabaseInitializationService(GameDbContext dbContext)
{
    /// <summary>
    /// Applies migrations and optionally drops the existing database before recreating it.
    /// </summary>
    /// <param name="resetDatabaseOnStartup">When true, clears application data after migrations are applied.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task InitializeAsync(bool resetDatabaseOnStartup, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (resetDatabaseOnStartup)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE moves, players, games, engine_capabilities RESTART IDENTITY CASCADE;",
                cancellationToken);
        }
    }
}
