using System.Reflection;

namespace MyProject.Endpoints;

/// <summary>
/// Health and version check endpoints.
/// </summary>
public static class HealthEndpoints
{
    /// <summary>
    /// Maps the health and version endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }))
           .WithName("Health")
           .WithSummary("Returns application health status");

        app.MapGet("/version", () => Results.Ok(new
        {
            version = Assembly.GetExecutingAssembly()
                              .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                              ?.InformationalVersion ?? "unknown",
        }))
           .WithName("Version")
           .WithSummary("Returns application version");
    }
}
