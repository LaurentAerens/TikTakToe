using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
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
    }

    [Fact]
    public async Task CreateEngineById_KnownAndUnknownIds()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var oppertunity = await provider.GetByDisplayNameAsync("Oppertunity");
        Assert.NotNull(oppertunity);

        var engine = await provider.CreateEngineByIdAsync(oppertunity!.Id);
        Assert.NotNull(engine);
        Assert.Equal("OppertunityEngine", engine!.GetType().Name);

        var missing = await provider.CreateEngineByIdAsync(Guid.NewGuid());
        Assert.Null(missing);
    }

    private static GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GameDbContext(options);
    }
}