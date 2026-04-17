using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Services;

namespace TikTakToe.Tests.Services;

public sealed class EngineLookupProviderTests
{
    [Fact]
    public async Task EnsureCapabilitiesAsync_CreatesOneCapabilityPerEngine()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);

        await provider.EnsureCapabilitiesAsync();

        var capabilities = await provider.ListCapabilitiesAsync();
        Assert.Equal(5, capabilities.Count);
        Assert.Contains(capabilities, x => x.DisplayName == "Classical" && x.Depth);
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
    public async Task SaveChangesAsync_ThrowsWhenExistingEnginePlayerExternalIdHasDifferentCasing()
    {
        await using var dbContext = CreateDbContext();
        var engineId = Guid.NewGuid();

        dbContext.Players.Add(new PlayerModel
        {
            Id = Guid.NewGuid(),
            IsEngine = true,
            ExternalId = engineId.ToString("D").ToUpperInvariant(),
        });
        await dbContext.SaveChangesAsync();

        dbContext.Players.Add(new PlayerModel
        {
            Id = Guid.NewGuid(),
            IsEngine = true,
            ExternalId = engineId.ToString("D"),
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.SaveChangesAsync());
        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GameDbContext(options);
    }
}