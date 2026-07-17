namespace TikTakToe.Tests.Controllers;

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TikTakToe.Controllers;
using TikTakToe.Engines.Interface;
using TikTakToe.Models;
using TikTakToe.Services;

public sealed class EngineLookupControllerTests : IDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;
    private readonly FakeEngineLookupProvider _provider;

    public EngineLookupControllerTests()
    {
        var engineId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var fakeEngine = new FakeEngine(supportedPlayers: [1, 2]);
        this._provider = new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                PlayerId = playerId,
                DisplayName = "Test Engine",
                MaxBoardSizeX = 3,
                MaxBoardSizeY = 3,
                Depth = true,
            },
            engine: fakeEngine);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IEngineLookupProvider>(this._provider);

        this._app = builder.Build();
        this._app.MapEngineLookupController();
        this._app.StartAsync().GetAwaiter().GetResult();

        this._client = this._app.GetTestClient();
    }

    public void Dispose()
    {
        this._client.Dispose();
        this._app.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ListEngines_ReturnsEnginesWithSupportedPlayers()
    {
        var response = await this._client.GetAsync("/engines");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        var engines = document.RootElement.GetProperty("data").EnumerateArray().ToArray();
        Assert.Single(engines);

        var engine = engines[0];
        Assert.Equal(this._provider.Capability!.Id, engine.GetProperty("id").GetGuid());
        Assert.Equal(this._provider.Capability.PlayerId, engine.GetProperty("playerId").GetGuid());
        Assert.Equal("Test Engine", engine.GetProperty("displayName").GetString());
        Assert.Equal(3, engine.GetProperty("maxBoardSizeX").GetInt32());
        Assert.Equal(3, engine.GetProperty("maxBoardSizeY").GetInt32());
        Assert.True(engine.GetProperty("depth").GetBoolean());

        var supportedPlayers = engine.GetProperty("supportedPlayers").EnumerateArray().Select(this.GetSupportedPlayer).ToArray();
        Assert.Equal(2, supportedPlayers.Length);
        Assert.Contains(1, supportedPlayers);
        Assert.Contains(2, supportedPlayers);
    }

    [Fact]
    public async Task GetEngineById_WithValidId_ReturnsEngine()
    {
        var engineId = this._provider.Capability!.Id;
        var response = await this._client.GetAsync($"/engines/{engineId}");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());

        var engine = document.RootElement.GetProperty("data");
        Assert.Equal(engineId, engine.GetProperty("id").GetGuid());
        Assert.Equal(this._provider.Capability.PlayerId, engine.GetProperty("playerId").GetGuid());
        Assert.Equal("Test Engine", engine.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task GetEngineById_WithInvalidId_ReturnsNotFound()
    {
        var response = await this._client.GetAsync($"/engines/{Guid.NewGuid()}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("Engine id not found", document.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GetEngineByDisplayName_WithValidName_ReturnsEngine()
    {
        var response = await this._client.GetAsync("/engines/Test Engine");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());

        var engine = document.RootElement.GetProperty("data");
        Assert.Equal(this._provider.Capability!.Id, engine.GetProperty("id").GetGuid());
        Assert.Equal("Test Engine", engine.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task GetEngineByDisplayName_WithInvalidName_ReturnsNotFound()
    {
        var response = await this._client.GetAsync("/engines/Unknown Engine");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("Engine display name not found", document.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task ResolveEngineIdByPlayerId_WithValidPlayerId_ReturnsEngine()
    {
        var playerId = this._provider.Capability!.PlayerId;
        var response = await this._client.GetAsync($"/engines/resolve-engine-id/{playerId}");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());

        var engine = document.RootElement.GetProperty("data");
        Assert.Equal(this._provider.Capability.Id, engine.GetProperty("id").GetGuid());
        Assert.Equal(playerId, engine.GetProperty("playerId").GetGuid());
    }

    [Fact]
    public async Task ResolveEngineIdByPlayerId_WithInvalidPlayerId_ReturnsNotFound()
    {
        var response = await this._client.GetAsync($"/engines/resolve-engine-id/{Guid.NewGuid()}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("Engine player id not found", document.RootElement.GetProperty("error").GetString());
    }

    private int GetSupportedPlayer(JsonElement element)
    {
        return element.GetInt32();
    }

    private sealed class FakeEngineLookupProvider(EngineCapabilityWithPlayerModel? capability, IEngine? engine) : IEngineLookupProvider
    {
        public EngineCapabilityWithPlayerModel? Capability { get; } = capability;

        public Task EnsureCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EngineCapabilityWithPlayerModel>> ListCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<EngineCapabilityWithPlayerModel> result = capability is null ? [] : [capability];
            return Task.FromResult(result);
        }

        public Task<EngineCapabilityWithPlayerModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(capability?.Id == id ? capability : null);
        }

        public Task<EngineCapabilityWithPlayerModel?> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(capability?.PlayerId == playerId ? capability : null);
        }

        public Task<EngineCapabilityWithPlayerModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(capability?.DisplayName == displayName ? capability : null);
        }

        public Task<IEngine?> CreateEngineByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(capability?.Id == id ? engine : null);
        }

        public IEngine? CreateEngineFromCapability(EngineCapabilityWithPlayerModel fetchedCapability)
        {
            return capability == fetchedCapability ? engine : null;
        }

        public Task<IEngine?> CreateEngineByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEngine?>(null);
        }

        public Task<IReadOnlyCollection<int>> GetSupportedPlayersByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var supportedPlayers = capability?.Id == id ? engine?.SupportedPlayers : null;
            return Task.FromResult<IReadOnlyCollection<int>>(supportedPlayers ?? [1, 2]);
        }
    }

    private sealed class FakeEngine(int[] supportedPlayers) : IEngine
    {
        public IReadOnlyCollection<int> SupportedPlayers { get; } = supportedPlayers;

        public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
        {
            throw new NotSupportedException("Move is not used in these tests.");
        }

        public int Eval(int[,] board, int player, int? depth = null)
        {
            throw new NotSupportedException("Eval is not used in these tests.");
        }
    }
}
