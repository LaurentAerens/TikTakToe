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

        foreach (var root in candidateRoots)
        {
            var resolved = FindContentRoot(root);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        return Directory.GetCurrentDirectory();
    }

    private static string? FindContentRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);

        while (current is not null)
        {
            var appSettingsPath = Path.Combine(current.FullName, "appsettings.json");
            var projectPath = Path.Combine(current.FullName, "TikTakToe.csproj");

            if (File.Exists(appSettingsPath) && File.Exists(projectPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}