using TikTakToe.Endpoints;
using TikTakToe.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IExampleService, ExampleService>();

var app = builder.Build();

// Map endpoints
app.MapHealthEndpoints();

app.Run();
