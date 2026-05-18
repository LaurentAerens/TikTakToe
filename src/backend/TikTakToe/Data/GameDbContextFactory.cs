namespace TikTakToe.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

/// <summary>
/// Design-time factory for EF Core migration commands.
/// </summary>
public sealed class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    /// <inheritdoc />
    public GameDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = ResolveContentRoot();

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true);

        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
        {
            configurationBuilder.AddUserSecrets<GameDbContextFactory>(optional: true);
        }

        var configuration = configurationBuilder
            .AddEnvironmentVariables()
            .Build();

        var databaseOptions = DatabaseConnectionOptions.FromConfiguration(configuration);
        var resolver = new DatabaseConnectionStringResolver(Options.Create(databaseOptions));
        var connectionString = resolver.Resolve();

        var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new GameDbContext(optionsBuilder.Options);
    }

    private static string ResolveContentRoot()
    {
        var candidateRoots = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory,
        };

        var resolvedRoot = candidateRoots
            .Select(FindContentRoot)
            .FirstOrDefault(path => path is not null);

        if (resolvedRoot is not null)
        {
            return resolvedRoot;
        }

        return Directory.GetCurrentDirectory();
    }

    private static string? FindContentRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);

        while (current is not null)
        {
            var hasAppSettings = current.GetFiles("appsettings.json", SearchOption.TopDirectoryOnly).Length > 0;
            var hasProjectFile = current.GetFiles("TikTakToe.csproj", SearchOption.TopDirectoryOnly).Length > 0;

            if (hasAppSettings && hasProjectFile)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}
