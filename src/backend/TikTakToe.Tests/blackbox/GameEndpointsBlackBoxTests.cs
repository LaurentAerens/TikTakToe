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

        var createPayload = new { rows = 4, cols = 5 };
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
        var board = getDocument.RootElement.GetProperty("data").GetProperty("board");

        Assert.Equal(4, board.GetArrayLength());
        foreach (var row in board.EnumerateArray())
        {
            Assert.Equal(5, row.GetArrayLength());
        }
    }
}