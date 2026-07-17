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

public sealed class EvalControllerTests : IDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;
    private readonly FakeEngineLookupProvider _provider;

    public EvalControllerTests()
    {
        var engineId = Guid.NewGuid();
        var fakeEngine = new FakeEngine(score: 100, supportedPlayers: [1, 2]);
        this._provider = new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Test Engine",
                MaxBoardSizeX = 3,
                MaxBoardSizeY = 3,
            },
            engine: fakeEngine);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IEvalService>(new EvalService(this._provider));

        this._app = builder.Build();
        this._app.MapEvalController();
        this._app.StartAsync().GetAwaiter().GetResult();

        this._client = this._app.GetTestClient();
    }

    public void Dispose()
    {
        this._client.Dispose();
        this._app.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Eval_WithValidRequest_ReturnsScore()
    {
        var engineId = this._provider.Capability!.Id;
        var payload = new
        {
            engineId,
            player = 1,
            board = new[]
            {
                new[] { 1, 0, 2 },
                new[] { 0, 1, 0 },
                new[] { 2, 0, 0 },
            },
        };

        var response = await this._client.PostAsJsonAsync("/eval", payload);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(100, document.RootElement.GetProperty("data").GetProperty("score").GetInt32());
    }

    [Fact]
    public async Task Eval_WithUnknownEngine_ReturnsNotFound()
    {
        var payload = new
        {
            engineId = Guid.NewGuid(),
            player = 1,
            board = new[]
            {
                new[] { 0, 0, 0 },
                new[] { 0, 0, 0 },
                new[] { 0, 0, 0 },
            },
        };

        var response = await this._client.PostAsJsonAsync("/eval", payload);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("Engine id not found", document.RootElement.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Eval_WithInvalidBoardShape_ReturnsBadRequest()
    {
        var engineId = this._provider.Capability!.Id;
        var payload = new
        {
            engineId,
            player = 1,
            board = new[]
            {
                new[] { 0, 0, 0 },
                new[] { 1, 0 },
            },
        };

        var response = await this._client.PostAsJsonAsync("/eval", payload);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.False(document.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("equal length", document.RootElement.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Eval_WithDepthParameter_ReturnsScore()
    {
        var engineId = this._provider.Capability!.Id;
        var payload = new
        {
            engineId,
            player = 1,
            depth = 5,
            board = new[]
            {
                new[] { 0, 0, 0 },
                new[] { 0, 0, 0 },
                new[] { 0, 0, 0 },
            },
        };

        var response = await this._client.PostAsJsonAsync("/eval", payload);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
    }

    private sealed class FakeEngineLookupProvider(
        EngineCapabilityWithPlayerModel? capability,
        IEngine? engine) : IEngineLookupProvider
    {
        private readonly IEngine? _engine = engine;

        public EngineCapabilityWithPlayerModel? Capability { get; } = capability;

        public Task EnsureCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EngineCapabilityWithPlayerModel>> ListCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<EngineCapabilityWithPlayerModel> result = this.Capability is null ? [] : [this.Capability];
            return Task.FromResult(result);
        }

        public Task<EngineCapabilityWithPlayerModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.Capability?.Id == id ? this.Capability : null);
        }

        public Task<EngineCapabilityWithPlayerModel?> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EngineCapabilityWithPlayerModel?>(null);
        }

        public Task<EngineCapabilityWithPlayerModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EngineCapabilityWithPlayerModel?>(null);
        }

        public Task<IEngine?> CreateEngineByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.Capability?.Id == id ? this._engine : null);
        }

        public IEngine? CreateEngineFromCapability(EngineCapabilityWithPlayerModel fetchedCapability)
        {
            return this.Capability == fetchedCapability ? this._engine : null;
        }

        public Task<IEngine?> CreateEngineByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEngine?>(null);
        }

        public Task<IReadOnlyCollection<int>> GetSupportedPlayersByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var supportedPlayers = this.Capability?.Id == id ? this._engine?.SupportedPlayers : null;
            return Task.FromResult<IReadOnlyCollection<int>>(supportedPlayers ?? [1, 2]);
        }
    }

    private sealed class FakeEngine(int score, int[] supportedPlayers) : IEngine
    {
        public IReadOnlyCollection<int> SupportedPlayers { get; } = supportedPlayers;

        public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
        {
            throw new NotSupportedException("Move is not used in eval tests.");
        }

        public int Eval(int[,] board, int player, int? depth = null)
        {
            return score;
        }
    }
}
