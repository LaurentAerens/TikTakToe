using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TikTakToe.Tests.BlackBox;

[Trait("Category", "BlackBox")]
[Collection(BlackBoxCollection.Name)]
public sealed class GameEndpointsBlackBoxTests(BlackBoxComposeFixture fixture)
{
    [BlackBoxFact]
    public async Task Healthz_ReturnsOk()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };

        using var response = await client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [BlackBoxFact]
    public async Task CreateAndGetGame_ReturnsExpectedBoardShape()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };
        var playerIds = await GetEnginePlayerIdsAsync(client, 2);

        var createPayload = new { rows = 4, cols = 5, playerIds };
        using var createResponse = await client.PostAsJsonAsync("/games", createPayload);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        await using var createStream = await createResponse.Content.ReadAsStreamAsync();
        using var createDocument = await JsonDocument.ParseAsync(createStream);
        var root = createDocument.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var gameId = root.GetProperty("data").GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, gameId);

        using var getResponse = await client.GetAsync($"/games/{gameId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        await using var getStream = await getResponse.Content.ReadAsStreamAsync();
        using var getDocument = await JsonDocument.ParseAsync(getStream);
        var data = getDocument.RootElement.GetProperty("data");
        var board = data.GetProperty("board");
        var players = data.GetProperty("players");

        Assert.Equal(4, board.GetArrayLength());
        foreach (var row in board.EnumerateArray())
        {
            Assert.Equal(5, row.GetArrayLength());
        }

        Assert.Equal(2, players.GetArrayLength());
    }

    [BlackBoxFact]
    public async Task CreateGame_WithTooLargeDimensions_ReturnsBadRequest()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };
        var playerIds = await GetEnginePlayerIdsAsync(client, 2);

        var createPayload = new { rows = 10001, cols = 3, playerIds };
        using var createResponse = await client.PostAsJsonAsync("/games", createPayload);

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);

        await using var stream = await createResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Contains("less than or equal to 10000", root.GetProperty("error").GetString());
    }

    [BlackBoxFact]
    public async Task CreateGame_WithSinglePlayerId_ReturnsBadRequest()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };
        var playerIds = await GetEnginePlayerIdsAsync(client, 1);

        var createPayload = new { rows = 3, cols = 3, playerIds };
        using var createResponse = await client.PostAsJsonAsync("/games", createPayload);

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);

        await using var stream = await createResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Contains("between 2 and 1000", root.GetProperty("error").GetString());
    }

    private static async Task<Guid[]> GetEnginePlayerIdsAsync(HttpClient client, int count)
    {
        using var enginesResponse = await client.GetAsync("/engines");
        enginesResponse.EnsureSuccessStatusCode();

        await using var stream = await enginesResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var engines = document.RootElement.GetProperty("data").EnumerateArray().ToArray();
        return engines
            .Take(count)
            .Select(x => x.GetProperty("playerId").GetGuid())
            .ToArray();
    }
}