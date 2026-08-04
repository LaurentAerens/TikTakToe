namespace TikTakToe.Tests.Services;

using Microsoft.EntityFrameworkCore;
using TikTakToe.Data;
using TikTakToe.Models;
using TikTakToe.Services;

public sealed class BotServiceTests
{
    [Fact]
    public async Task CreateBotAsync_CreatesPlayerAndBot()
    {
        await using var dbContext = CreateDbContext();
        var capability = new EngineCapabilityModel
        {
            Id = Guid.NewGuid(),
            DisplayName = "Classical",
            MaxBoardSizeX = 3,
            MaxBoardSizeY = 3,
            Depth = true,
        };
        dbContext.EngineCapabilities.Add(capability);
        await dbContext.SaveChangesAsync();

        var service = new BotService(dbContext);
        var bot = await service.CreateBotAsync(capability.Id, "My Custom Bot", depth: 2);

        Assert.NotNull(bot);
        Assert.NotEqual(Guid.Empty, bot.Id);
        Assert.Equal(capability.Id, bot.EngineCapabilityId);
        Assert.Equal("My Custom Bot", bot.DisplayName);
        Assert.Equal(2, bot.Depth);

        var player = await dbContext.Players.SingleOrDefaultAsync(p => p.Id == bot.Id);
        Assert.NotNull(player);
        Assert.True(player!.IsEngine);
    }

    [Fact]
    public async Task EnsureDefaultBotsAsync_SeedsDefaultBotsForCapabilities()
    {
        await using var dbContext = CreateDbContext();
        var provider = new EngineLookupProvider(dbContext);
        await provider.EnsureCapabilitiesAsync();

        var botService = new BotService(dbContext);
        var bots = await botService.ListBotsAsync();

        // 10 minimax engines * 9 depths + 1 random engine = 91 bots
        Assert.Equal(91, bots.Count);
        Assert.Contains(bots, b => b.DisplayName == "Classical (Depth 1)" && b.Depth == 1);
        Assert.Contains(bots, b => b.DisplayName == "Classical (Depth 9)" && b.Depth == 9);
        Assert.Contains(bots, b => b.DisplayName == "Random" && b.Depth == null);
    }

    private static GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GameDbContext(options);
    }
}
