namespace TikTakToe.Tests.Services;

using TikTakToe.Engines.Interface;
using TikTakToe.Models;
using TikTakToe.Services;

public sealed class EvalServiceTests
{
    [Fact]
    public async Task EvaluateAsync_WithValidInput_ReturnsEngineScore()
    {
        var engineId = Guid.NewGuid();
        var fakeEngine = new FakeEngine(score: 77, supportedPlayers: [1, 2]);
        var provider = new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Test Engine",
                MaxBoardSizeX = 3,
                MaxBoardSizeY = 3,
            },
            engine: fakeEngine);

        var service = new EvalService(provider);
        var score = await service.EvaluateAsync(
            engineId,
            [
                [1, 0, 2],
                [0, 1, 0],
                [2, 0, 0],
            ],
            player: 1);

        Assert.Equal(77, score);
        Assert.Equal(1, fakeEngine.LastPlayer);
        Assert.NotNull(fakeEngine.LastBoard);
        Assert.Equal(3, fakeEngine.LastBoard!.GetLength(0));
        Assert.Equal(3, fakeEngine.LastBoard.GetLength(1));
        Assert.Null(fakeEngine.LastDepth);
    }

    [Fact]
    public async Task EvaluateAsync_WithUnknownEngine_ThrowsKeyNotFoundException()
    {
        var service = new EvalService(new FakeEngineLookupProvider(capability: null, engine: null));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.EvaluateAsync(Guid.NewGuid(), [[0]], player: 1));
    }

    [Fact]
    public async Task EvaluateAsync_WithNonRectangularBoard_ThrowsArgumentException()
    {
        var engineId = Guid.NewGuid();
        var service = new EvalService(new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Test Engine",
                MaxBoardSizeX = 10,
                MaxBoardSizeY = 10,
            },
            engine: new FakeEngine(score: 1, supportedPlayers: [1, 2])));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.EvaluateAsync(
            engineId,
            [
                [0, 0],
                [1],
            ],
            player: 1));

        Assert.Contains("equal length", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EvaluateAsync_WithBoardExceedingCapability_ThrowsArgumentException()
    {
        var engineId = Guid.NewGuid();
        var service = new EvalService(new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Test Engine",
                MaxBoardSizeX = 2,
                MaxBoardSizeY = 2,
            },
            engine: new FakeEngine(score: 1, supportedPlayers: [1, 2])));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.EvaluateAsync(
            engineId,
            [
                [0, 0, 0],
                [0, 1, 0],
                [2, 0, 0],
            ],
            player: 1));

        Assert.Contains("exceed engine limits", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EvaluateAsync_WithUnsupportedPlayer_ThrowsArgumentOutOfRangeException()
    {
        var engineId = Guid.NewGuid();
        var service = new EvalService(new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Test Engine",
                MaxBoardSizeX = 3,
                MaxBoardSizeY = 3,
            },
            engine: new FakeEngine(score: 1, supportedPlayers: [1, 2])));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.EvaluateAsync(engineId, [[0, 0], [0, 0]], player: 3));
    }

    [Fact]
    public async Task EvaluateAsync_WithDepthZero_UsesEngineDefaultDepth()
    {
        var engineId = Guid.NewGuid();
        var fakeEngine = new FakeEngine(score: 7, supportedPlayers: [1, 2]);
        var service = new EvalService(new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Test Engine",
                MaxBoardSizeX = 3,
                MaxBoardSizeY = 3,
            },
            engine: fakeEngine));

        _ = await service.EvaluateAsync(engineId, [[0, 0, 0], [0, 0, 0], [0, 0, 0]], player: 1, depth: 0);

        Assert.Null(fakeEngine.LastDepth);
    }

    [Fact]
    public async Task EvaluateAsync_WithPositiveDepth_PassesDepthToEngine()
    {
        var engineId = Guid.NewGuid();
        var fakeEngine = new FakeEngine(score: 7, supportedPlayers: [1, 2]);
        var service = new EvalService(new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Test Engine",
                MaxBoardSizeX = 3,
                MaxBoardSizeY = 3,
            },
            engine: fakeEngine));

        _ = await service.EvaluateAsync(engineId, [[0, 0, 0], [0, 0, 0], [0, 0, 0]], player: 1, depth: 4);

        Assert.Equal(4, fakeEngine.LastDepth);
    }

    [Fact]
    public async Task EvaluateAsync_WithBoardContainingUnsupportedPlayer_ThrowsArgumentException()
    {
        var engineId = Guid.NewGuid();
        var service = new EvalService(new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Future Engine",
                MaxBoardSizeX = 3,
                MaxBoardSizeY = 3,
            },
            engine: new FakeEngine(score: 1, supportedPlayers: [1, 3])));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.EvaluateAsync(
            engineId,
            [
                [1, 0, 3],
                [0, 2, 0],
                [3, 0, 1],
            ],
            player: 1));

        Assert.Contains("unsupported player value", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EvaluateAsync_WithBoardExceedingMaxCells_ThrowsArgumentException()
    {
        var engineId = Guid.NewGuid();
        var service = new EvalService(new FakeEngineLookupProvider(
            capability: new EngineCapabilityWithPlayerModel
            {
                Id = engineId,
                DisplayName = "Random Engine",
                MaxBoardSizeX = 10000,
                MaxBoardSizeY = 10000,
            },
            engine: new FakeEngine(score: 0, supportedPlayers: [1, 2])));

        // Create a board with 1001 x 1001 = 1,002,001 cells (exceeds 1M cell limit)
        var largeBoard = new int[1001][];
        for (int i = 0; i < 1001; i++)
        {
            largeBoard[i] = new int[1001];
        }

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.EvaluateAsync(engineId, largeBoard, player: 1));

        Assert.Contains("maximum allowed cells", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeEngineLookupProvider(EngineCapabilityWithPlayerModel? capability, IEngine? engine) : IEngineLookupProvider
    {
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
            return Task.FromResult<EngineCapabilityWithPlayerModel?>(null);
        }

        public Task<EngineCapabilityWithPlayerModel?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EngineCapabilityWithPlayerModel?>(null);
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

    private sealed class FakeEngine(int score, int[] supportedPlayers) : IEngine
    {
        public int[,]? LastBoard { get; private set; }

        public int LastPlayer { get; private set; }

        public int? LastDepth { get; private set; }

        public IReadOnlyCollection<int> SupportedPlayers { get; } = supportedPlayers;

        public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
        {
            throw new NotSupportedException("Move is not used in eval tests.");
        }

        public int Eval(int[,] board, int player, int? depth = null)
        {
            this.LastBoard = board;
            this.LastPlayer = player;
            this.LastDepth = depth;
            return score;
        }
    }
}
