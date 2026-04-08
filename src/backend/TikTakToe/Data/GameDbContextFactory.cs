using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TikTakToe.Data;

/// <summary>
/// Design-time factory for EF Core migration commands.
/// </summary>
public sealed class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    /// <inheritdoc />
    public GameDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = DatabaseConnectionStringResolver.Resolve(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new GameDbContext(optionsBuilder.Options);
    }
}