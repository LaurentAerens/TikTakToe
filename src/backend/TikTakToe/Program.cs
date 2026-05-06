using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TikTakToe.Controllers;
using TikTakToe.Data;
using TikTakToe.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddOptions<DatabaseConnectionOptions>()
    .Configure<IConfiguration>((options, configuration) =>
    {
        var configuredOptions = DatabaseConnectionOptions.FromConfiguration(configuration);
        options.DefaultConnection = configuredOptions.DefaultConnection;
        options.Host = configuredOptions.Host;
        options.Database = configuredOptions.Database;
        options.Username = configuredOptions.Username;
        options.Password = configuredOptions.Password;
        options.Port = configuredOptions.Port;
    });
builder.Services.AddSingleton<IDatabaseConnectionStringResolver, DatabaseConnectionStringResolver>();

builder.Services.AddDbContext<GameDbContext>((serviceProvider, options) =>
{
    var connectionStringResolver = serviceProvider.GetRequiredService<IDatabaseConnectionStringResolver>();
    options.UseNpgsql(connectionStringResolver.Resolve());
});
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IEvalService, EvalService>();
builder.Services.AddScoped<IEngineLookupProvider, EngineLookupProvider>();
builder.Services.AddOpenApi();

var app = builder.Build();
var exposeApiDocs = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Features:ExposeApiDocs");
var applyMigrationsOnStartup = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Features:ApplyMigrationsOnStartup");

if (applyMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    dbContext.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var engineLookupProvider = scope.ServiceProvider.GetRequiredService<IEngineLookupProvider>();
    await engineLookupProvider.EnsureCapabilitiesAsync();
}

// Map endpoints
app.MapHealthController();
app.MapGameController();
app.MapEvalController();
app.MapEngineLookupController();
if (exposeApiDocs)
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();
