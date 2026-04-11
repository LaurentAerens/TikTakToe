using TikTakToe.Controllers;
using TikTakToe.Data;
using TikTakToe.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IExampleService, ExampleService>();
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
builder.Services.AddOpenApi();

var app = builder.Build();
var exposeApiDocs = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Features:ExposeApiDocs");

if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();
	var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
	dbContext.Database.Migrate();
}

// Map endpoints
app.MapHealthController();
app.MapGameController();
if (exposeApiDocs)
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.Run();
