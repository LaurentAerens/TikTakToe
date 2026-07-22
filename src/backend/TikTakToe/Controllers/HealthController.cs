namespace TikTakToe.Controllers;

using System.Reflection;

/// <summary>
/// Health and version controller mappings.
/// </summary>
public static class HealthController
{
    /// <summary>
    /// Maps the health and version controller routes to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapHealthController(this WebApplication app)
    {
        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }))
              .WithName("CheckApiHealth")
              .WithSummary("Check API health");

        app.MapGet("/version", () => Results.Ok(new
        {
            version = Assembly.GetExecutingAssembly()
                              .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                              ?.InformationalVersion ?? "unknown",
        }))
           .WithName("CheckApiVersion")
           .WithSummary("Check API version");
    }
}
