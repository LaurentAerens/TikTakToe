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
        var basePath = ResolveContentRoot();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = DatabaseConnectionStringResolver.Resolve(configuration);

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