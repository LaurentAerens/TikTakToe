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

    [BlackBoxFact]
    public async Task Eval_WithValidPayload_ReturnsScore()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };
        var engineId = await GetEngineIdByDisplayNameAsync(client, "Classical");

        var payload = new
        {
            engineId,
            player = 1,
            board = new[]
            {
                new[] { 1, 1, 1 },
                new[] { 0, 2, 0 },
                new[] { 2, 0, 2 },
            },
        };

        using var evalResponse = await client.PostAsJsonAsync("/eval", payload);

        Assert.Equal(HttpStatusCode.OK, evalResponse.StatusCode);

        await using var stream = await evalResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var score = root.GetProperty("data").GetProperty("score").GetInt32();
        Assert.Equal(1000, score);
    }

    [BlackBoxFact]
    public async Task Eval_WithUnknownEngine_ReturnsNotFound()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };

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

        using var evalResponse = await client.PostAsJsonAsync("/eval", payload);

        Assert.Equal(HttpStatusCode.NotFound, evalResponse.StatusCode);

        await using var stream = await evalResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Contains("Engine id not found", root.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [BlackBoxFact]
    public async Task Eval_WithInvalidBoardShape_ReturnsBadRequest()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };
        var engineId = await GetEngineIdByDisplayNameAsync(client, "Classical");

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

        using var evalResponse = await client.PostAsJsonAsync("/eval", payload);

        Assert.Equal(HttpStatusCode.BadRequest, evalResponse.StatusCode);

        await using var stream = await evalResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Contains("equal length", root.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [BlackBoxFact]
    public async Task Eval_WithOpportunityEngine_ImmediateWinPosition_ReturnsWinningScore()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };
        var engineId = await GetEngineIdByDisplayNameAsync(client, "Opportunity");

        var payload = new
        {
            engineId,
            player = 1,
            board = new[]
            {
                new[] { 1, 1, 0 },
                new[] { 2, 1, 2 },
                new[] { 2, 0, 0 },
            },
        };

        using var evalResponse = await client.PostAsJsonAsync("/eval", payload);

        Assert.Equal(HttpStatusCode.OK, evalResponse.StatusCode);

        await using var stream = await evalResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var root = document.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var score = root.GetProperty("data").GetProperty("score").GetInt32();
        Assert.Equal(1000, score);
    }

    [BlackBoxFact]
    public async Task Eval_WithDepthZero_MatchesEvalWithoutDepth()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };
        var engineId = await GetEngineIdByDisplayNameAsync(client, "Opportunity");

        var payloadNoDepth = new
        {
            engineId,
            player = 1,
            board = new[]
            {
                new[] { 1, 1, 0 },
                new[] { 2, 1, 2 },
                new[] { 2, 0, 0 },
            },
        };

        var payloadDepthZero = new
        {
            engineId,
            player = 1,
            depth = 0,
            board = new[]
            {
                new[] { 1, 1, 0 },
                new[] { 2, 1, 2 },
                new[] { 2, 0, 0 },
            },
        };

        using var noDepthResponse = await client.PostAsJsonAsync("/eval", payloadNoDepth);
        using var zeroDepthResponse = await client.PostAsJsonAsync("/eval", payloadDepthZero);

        noDepthResponse.EnsureSuccessStatusCode();
        zeroDepthResponse.EnsureSuccessStatusCode();

        var noDepthScore = await ReadEvalScoreAsync(noDepthResponse);
        var zeroDepthScore = await ReadEvalScoreAsync(zeroDepthResponse);
        Assert.Equal(noDepthScore, zeroDepthScore);
    }

    [BlackBoxFact]
    public async Task ListEngines_IncludesSupportedPlayers()
    {
        using var client = new HttpClient { BaseAddress = fixture.BaseAddress };

        using var enginesResponse = await client.GetAsync("/engines");
        enginesResponse.EnsureSuccessStatusCode();

        await using var stream = await enginesResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var engines = document.RootElement.GetProperty("data").EnumerateArray().ToArray();

        Assert.NotEmpty(engines);
        foreach (var engine in engines)
        {
            var supportedPlayers = engine.GetProperty("supportedPlayers");
            Assert.True(supportedPlayers.GetArrayLength() >= 2);
            Assert.Contains(supportedPlayers.EnumerateArray().Select(x => x.GetInt32()), x => x == 1);
            Assert.Contains(supportedPlayers.EnumerateArray().Select(x => x.GetInt32()), x => x == 2);
        }
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

    private static async Task<Guid> GetEngineIdByDisplayNameAsync(HttpClient client, string displayName)
    {
        using var enginesResponse = await client.GetAsync("/engines");
        enginesResponse.EnsureSuccessStatusCode();

        await using var stream = await enginesResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var engine = document.RootElement.GetProperty("data")
            .EnumerateArray()
            .Single(x => string.Equals(x.GetProperty("displayName").GetString(), displayName, StringComparison.Ordinal));

        return engine.GetProperty("id").GetGuid();
    }

    private static async Task<int> ReadEvalScoreAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.GetProperty("data").GetProperty("score").GetInt32();
    }
}