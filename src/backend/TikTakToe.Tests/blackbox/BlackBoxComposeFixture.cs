using System.Diagnostics;
using System.Net;

namespace TikTakToe.Tests.BlackBox;

public sealed class BlackBoxComposeFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(2);
    private const string ComposeBaseArguments = "compose -p tiktaktoe-blackbox -f docker-compose.yml --profile test";
    private readonly string repositoryRoot;

    public BlackBoxComposeFixture()
    {
        repositoryRoot = FindRepositoryRoot();
    }

    public Uri BaseAddress { get; } = new("http://localhost:8080");

    public async Task InitializeAsync()
    {
        if (BlackBoxTestSettings.ShouldSkip())
        {
            return;
        }

        await RunComposeAsync("up -d --build backend");
        await WaitForHealthAsync();
    }

    public async Task DisposeAsync()
    {
        if (BlackBoxTestSettings.ShouldSkip())
        {
            return;
        }

        await RunComposeAsync("down --remove-orphans -v");
    }

    private async Task WaitForHealthAsync()
    {
        using var client = new HttpClient { BaseAddress = BaseAddress };
        var deadline = DateTime.UtcNow + StartupTimeout;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync("/healthz");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch
            {
                // Service is still starting.
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException("Backend did not become healthy within the expected startup window.");
    }

    private async Task RunComposeAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"{ComposeBaseArguments} {arguments}",
            WorkingDirectory = repositoryRoot,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // Enable migrations for test profile to ensure database schema is up-to-date
        // Must copy all existing environment variables and add our override
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            var key = entry.Key?.ToString();
            var value = entry.Value?.ToString();
            if (!string.IsNullOrEmpty(key) && value is not null)
            {
                startInfo.EnvironmentVariables[key] = value;
            }
        }
        startInfo.EnvironmentVariables["FEATURES__APPLYMIGRATIONSONSTARTUP"] = "true";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start docker compose process.");

        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"docker compose failed with exit code {process.ExitCode}. Output: {standardOutput}\nErrors: {standardError}");
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var composeFilePath = Path.Combine(current.FullName, "docker-compose.yml");
            if (File.Exists(composeFilePath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing docker-compose.yml.");
    }
}