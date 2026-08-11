namespace TikTakToe.Tests.Services;

using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Services;

public sealed class EngineLookupProviderTests
{
    [Fact]
    public async Task EnsureCapabilitiesAsync_CreatesOneCapabilityPerEngine()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);

        await provider.EnsureCapabilitiesAsync();

        var capabilities = await provider.ListCapabilitiesAsync();
        Assert.Equal(11, capabilities.Count);
        Assert.Contains(capabilities, x => x.DisplayName == "Classical" && x.Depth);
        Assert.Contains(capabilities, x => x.DisplayName == "Inverse" && x.Depth);
        Assert.Contains(capabilities, x => x.DisplayName == "Sightline" && x.Depth);
        Assert.Contains(capabilities, x => x.DisplayName == "Blindsight" && x.Depth);
        Assert.Contains(capabilities, x => x.DisplayName == "Random" && !x.Depth);
        Assert.All(capabilities, x => Assert.NotEqual(Guid.Empty, x.Id));
        Assert.All(capabilities, x => Assert.NotEqual(Guid.Empty, x.PlayerId));

        var enginePlayers = await dbContext.Players.Where(x => x.IsEngine).ToListAsync();
        Assert.Equal(capabilities.Count, enginePlayers.Count);
        foreach (var capability in capabilities)
        {
            Assert.Contains(enginePlayers, p => p.Id == capability.PlayerId && p.ExternalId == capability.Id.ToString("D"));
        }
    }

    [Fact]
    public async Task ResolveByIdAndDisplayName_ReturnsSameCapability()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var halfDepth = await provider.GetByDisplayNameAsync("Half Depth");
        Assert.NotNull(halfDepth);

        var byId = await provider.GetByIdAsync(halfDepth!.Id);
        Assert.NotNull(byId);

        var byDisplayName = await provider.GetByDisplayNameAsync(byId!.DisplayName);
        Assert.NotNull(byDisplayName);
        Assert.Equal(byId.Id, byDisplayName!.Id);
        Assert.Equal(byId.PlayerId, byDisplayName.PlayerId);

        var byPlayerId = await provider.GetByPlayerIdAsync(byId.PlayerId);
        Assert.NotNull(byPlayerId);
        Assert.Equal(byId.Id, byPlayerId!.Id);
    }

    [Theory]
    [InlineData("hAlF dEpTh")]
    [InlineData("half-depth")]
    [InlineData("Half_depth")]
    [InlineData("half\tdepth")]
    public async Task GetByDisplayNameAsync_IsCaseAndSeparatorInsensitive(string lookupName)
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var canonical = await provider.GetByDisplayNameAsync("Half Depth");
        var variant = await provider.GetByDisplayNameAsync(lookupName);

        Assert.NotNull(canonical);
        Assert.NotNull(variant);
        Assert.Equal(canonical!.Id, variant!.Id);
        Assert.Equal(canonical.PlayerId, variant.PlayerId);
    }

    [Fact]
    public async Task CreateEngineById_KnownAndUnknownIds()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var opportunity = await provider.GetByDisplayNameAsync("Opportunity");
        Assert.NotNull(opportunity);

        var engine = await provider.CreateEngineByIdAsync(opportunity!.Id);
        Assert.NotNull(engine);
        Assert.Equal("OpportunityEngine", engine!.GetType().Name);

        var engineByPlayerId = await provider.CreateEngineByPlayerIdAsync(opportunity.PlayerId);
        Assert.NotNull(engineByPlayerId);
        Assert.Equal("OpportunityEngine", engineByPlayerId!.GetType().Name);

        var disconnicament = await provider.GetByDisplayNameAsync("Disconnicament");
        Assert.NotNull(disconnicament);
        var weakEngine = await provider.CreateEngineByIdAsync(disconnicament!.Id);
        Assert.NotNull(weakEngine);
        Assert.Equal("DisconnicamentEngine", weakEngine!.GetType().Name);

        var blindsight = await provider.GetByDisplayNameAsync("Blindsight");
        Assert.NotNull(blindsight);
        var blindsightEngine = await provider.CreateEngineByIdAsync(blindsight!.Id);
        Assert.NotNull(blindsightEngine);
        Assert.Equal("BlindsightEngine", blindsightEngine!.GetType().Name);

        var missing = await provider.CreateEngineByIdAsync(Guid.NewGuid());
        Assert.Null(missing);
    }

    [Fact]
    public async Task CreateEngineByIdAsync_MatchesRegistrationUsingNormalizedDisplayName()
    {
        await using var dbContext = CreateDbContext();
        var id = Guid.NewGuid();
        dbContext.EngineCapabilities.Add(new EngineCapabilityModel
        {
            Id = id,
            DisplayName = "half-depth",
            MaxBoardSizeX = 3,
            MaxBoardSizeY = 3,
            Depth = true,
        });
        await dbContext.SaveChangesAsync();

        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();
        var engine = await provider.CreateEngineByIdAsync(id);

        Assert.NotNull(engine);
        Assert.Equal("HalfDepthEngine", engine!.GetType().Name);
    }

    [Fact]
    public async Task SaveChangesAsync_ThrowsWhenCreatingTwoCapabilitiesWithSameNormalizedDisplayName()
    {
        await using var dbContext = CreateDbContext();
        dbContext.EngineCapabilities.Add(new EngineCapabilityModel
        {
            Id = Guid.NewGuid(),
            DisplayName = "Half Depth",
            MaxBoardSizeX = 3,
            MaxBoardSizeY = 3,
            Depth = true,
        });
        await dbContext.SaveChangesAsync();

        dbContext.EngineCapabilities.Add(new EngineCapabilityModel
        {
            Id = Guid.NewGuid(),
            DisplayName = "half_depth",
            MaxBoardSizeX = 3,
            MaxBoardSizeY = 3,
            Depth = true,
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.SaveChangesAsync());

        Assert.Contains("already exists under normalization rules", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveChangesAsync_ThrowsWhenCreatingTwoEnginePlayersForSameEngineExternalId()
    {
        await using var dbContext = CreateDbContext();
        var engineId = Guid.NewGuid().ToString("D");

        dbContext.Players.Add(new PlayerModel
        {
            Id = Guid.NewGuid(),
            IsEngine = true,
            ExternalId = engineId,
        });
        await dbContext.SaveChangesAsync();

        dbContext.Players.Add(new PlayerModel
        {
            Id = Guid.NewGuid(),
            IsEngine = true,
            ExternalId = engineId.ToUpperInvariant(),
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.SaveChangesAsync());
        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnsureCapabilitiesAsync_IsIdempotent()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);

        await provider.EnsureCapabilitiesAsync();
        await provider.EnsureCapabilitiesAsync();

        var capabilities = await provider.ListCapabilitiesAsync();
        var enginePlayers = await dbContext.Players.Where(x => x.IsEngine).ToListAsync();

        Assert.Equal(11, capabilities.Count);
        Assert.Equal(11, enginePlayers.Count);
        Assert.Equal(11, await dbContext.EngineCapabilities.CountAsync());
    }

    [Fact]
    public async Task EnsureCapabilitiesAsync_UpdatesStaleCapabilityMetadata()
    {
        await using var dbContext = CreateDbContext();
        dbContext.EngineCapabilities.Add(new EngineCapabilityModel
        {
            Id = Guid.NewGuid(),
            DisplayName = "Classical",
            MaxBoardSizeX = 1,
            MaxBoardSizeY = 1,
            Depth = false,
        });
        await dbContext.SaveChangesAsync();

        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var classical = await provider.GetByDisplayNameAsync("Classical");
        Assert.NotNull(classical);
        Assert.Equal(3, classical!.MaxBoardSizeX);
        Assert.Equal(3, classical.MaxBoardSizeY);
        Assert.True(classical.Depth);
        Assert.Equal(11, await dbContext.EngineCapabilities.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var result = await provider.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByDisplayNameAsync_WithUnknownName_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var result = await provider.GetByDisplayNameAsync("Not An Engine");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WithUnknownPlayer_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var result = await provider.GetByPlayerIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WithHumanPlayer_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var humanPlayerId = Guid.NewGuid();
        dbContext.Players.Add(new PlayerModel
        {
            Id = humanPlayerId,
            IsEngine = false,
            ExternalId = null,
        });
        await dbContext.SaveChangesAsync();

        var provider = new EngineLookupProvider(dbContext);
        var result = await provider.GetByPlayerIdAsync(humanPlayerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WithEnginePlayerForMissingCapability_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var playerId = Guid.NewGuid();
        dbContext.Players.Add(new PlayerModel
        {
            Id = playerId,
            IsEngine = true,
            ExternalId = Guid.NewGuid().ToString("D"),
        });
        await dbContext.SaveChangesAsync();

        var provider = new EngineLookupProvider(dbContext);
        var result = await provider.GetByPlayerIdAsync(playerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateEngineByPlayerIdAsync_WithUnknownPlayer_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var engine = await provider.CreateEngineByPlayerIdAsync(Guid.NewGuid());

        Assert.Null(engine);
    }

    [Fact]
    public async Task CreateEngineFromCapability_WithNullCapability_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);

        var engine = provider.CreateEngineFromCapability(null!);

        Assert.Null(engine);
    }

    [Fact]
    public async Task CreateEngineFromCapability_WithUnregisteredDisplayName_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);

        var engine = provider.CreateEngineFromCapability(new EngineCapabilityWithPlayerModel
        {
            Id = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            DisplayName = "Not A Registered Engine",
            MaxBoardSizeX = 3,
            MaxBoardSizeY = 3,
            Depth = true,
        });

        Assert.Null(engine);
    }

    [Fact]
    public async Task GetSupportedPlayersByIdAsync_KnownAndUnknownIds_ReturnDefaultTwoPlayers()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var classical = await provider.GetByDisplayNameAsync("Classical");
        Assert.NotNull(classical);

        var known = await provider.GetSupportedPlayersByIdAsync(classical!.Id);
        var unknown = await provider.GetSupportedPlayersByIdAsync(Guid.NewGuid());

        Assert.Equal([1, 2], known);
        Assert.Equal([1, 2], unknown);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCapabilityHasNoMappedPlayer_Throws()
    {
        await using var dbContext = CreateDbContext();
        var capabilityId = Guid.NewGuid();
        dbContext.EngineCapabilities.Add(new EngineCapabilityModel
        {
            Id = capabilityId,
            DisplayName = "Orphan Engine",
            MaxBoardSizeX = 3,
            MaxBoardSizeY = 3,
            Depth = true,
        });
        await dbContext.SaveChangesAsync();

        var provider = new EngineLookupProvider(dbContext);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetByIdAsync(capabilityId));
        Assert.Contains("has no mapped engine player", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListCapabilitiesAsync_WhenCapabilityHasNoMappedPlayer_Throws()
    {
        await using var dbContext = CreateDbContext();
        dbContext.EngineCapabilities.Add(new EngineCapabilityModel
        {
            Id = Guid.NewGuid(),
            DisplayName = "Orphan Engine",
            MaxBoardSizeX = 3,
            MaxBoardSizeY = 3,
            Depth = true,
        });
        await dbContext.SaveChangesAsync();

        var provider = new EngineLookupProvider(dbContext);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.ListCapabilitiesAsync());
        Assert.Contains("has no mapped engine player", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GameDbContext(options);
    }
}
