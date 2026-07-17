namespace TikTakToe.Tests.Controllers;

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TikTakToe.Controllers;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Services;

public sealed class GameControllerTests : IDisposable
{
    private readonly GameDbContext _dbContext;
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public GameControllerTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        this._dbContext = new GameDbContext(options);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        // Singleton so the request scope does not dispose the shared test context after each call.
        builder.Services.AddSingleton(this._dbContext);
        builder.Services.AddScoped<IGameService, GameService>();

        this._app = builder.Build();
        this._app.MapGameController();
        this._app.StartAsync().GetAwaiter().GetResult();

        this._client = this._app.GetTestClient();
    }

    public void Dispose()
    {
        this._client.Dispose();
        // Disposing the app also disposes the singleton GameDbContext.
        this._app.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreateGame_WithValidRequest_ReturnsCreated()
    {
        var playerIds = await this.SeedPlayersAsync(2);
        var payload = new { rows = 3, cols = 3, playerIds };

        var response = await this._client.PostAsJsonAsync("/games", payload);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.NotEqual(Guid.Empty, document.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task CreateGame_WithDefaultDimensions_UsesDefaults()
    {
        var playerIds = await this.SeedPlayersAsync(2);
        var payload = new { playerIds };

        var response = await this._client.PostAsJsonAsync("/games", payload);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        var board = document.RootElement.GetProperty("data").GetProperty("board");
        Assert.Equal(3, board.GetArrayLength());
        foreach (var row in board.EnumerateArray())
        {
            Assert.Equal(3, row.GetArrayLength());
        }
    }

    [Fact]
    public async Task CreateGame_WithTooLargeDimensions_ReturnsBadRequest()
    {
        var playerIds = await this.SeedPlayersAsync(2);
        var payload = new { rows = 10001, cols = 3, playerIds };

        var response = await this._client.PostAsJsonAsync("/games", payload);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("less than or equal to 10000", document.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task CreateGame_WithSinglePlayer_ReturnsBadRequest()
    {
        var playerIds = await this.SeedPlayersAsync(1);
        var payload = new { rows = 3, cols = 3, playerIds };

        var response = await this._client.PostAsJsonAsync("/games", payload);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("between 2 and 1000", document.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task CreateGame_WithTooManyPlayers_ReturnsBadRequest()
    {
        var playerIds = await this.SeedPlayersAsync(1001);
        var payload = new { rows = 3, cols = 3, playerIds };

        var response = await this._client.PostAsJsonAsync("/games", payload);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("between 2 and 1000", document.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task CreateGame_WithUnknownPlayerId_ReturnsBadRequest()
    {
        var knownPlayerId = await this.SeedPlayersAsync(1);
        var unknownPlayerId = Guid.NewGuid();
        var playerIds = new[] { knownPlayerId[0], unknownPlayerId };
        var payload = new { rows = 3, cols = 3, playerIds };

        var response = await this._client.PostAsJsonAsync("/games", payload);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task GetGame_WithValidId_ReturnsGame()
    {
        var playerIds = await this.SeedPlayersAsync(2);
        var createPayload = new { rows = 4, cols = 5, playerIds };
        var createResponse = await this._client.PostAsJsonAsync("/games", createPayload);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createDocument = JsonDocument.Parse(createContent);
        var gameId = createDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var getResponse = await this._client.GetAsync($"/games/{gameId}");

        Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.StatusCode);
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getDocument = JsonDocument.Parse(getContent);
        Assert.True(getDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(gameId, getDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task GetGame_WithInvalidId_ReturnsNotFound()
    {
        var response = await this._client.GetAsync($"/games/{Guid.NewGuid()}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("Game not found", document.RootElement.GetProperty("error").GetString());
    }

    private async Task<Guid[]> SeedPlayersAsync(int count)
    {
        var players = Enumerable.Range(0, count)
            .Select(_ => new PlayerModel
            {
                Id = Guid.NewGuid(),
                IsEngine = false,
                ExternalId = null,
            })
            .ToArray();

        this._dbContext.Players.AddRange(players);
        await this._dbContext.SaveChangesAsync();
        return players.Select(x => x.Id).ToArray();
    }
}
