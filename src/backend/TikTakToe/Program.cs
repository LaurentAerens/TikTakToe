using TikTakToe.Endpoints;
using TikTakToe.Data;
using TikTakToe.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IExampleService, ExampleService>();

var connectionString = DatabaseConnectionStringResolver.Resolve(builder.Configuration);

builder.Services.AddDbContext<GameDbContext>(options =>
	options.UseNpgsql(connectionString)
		   .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddSingleton(sp =>
{
	var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
	return dataSourceBuilder.Build();
});

builder.Services.AddScoped<IGameBoardStore, NpgsqlGameBoardStore>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();
	var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
	dbContext.Database.Migrate();
}

// Map endpoints
app.MapHealthEndpoints();
app.MapGameEndpoints();
app.MapOpenApi();
app.MapScalarApiReference();

app.Run();
