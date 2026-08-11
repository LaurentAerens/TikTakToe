namespace TikTakToe.Tests.Controllers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using TikTakToe.Controllers;

public sealed class HealthControllerTests : IDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public HealthControllerTests()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        this._app = builder.Build();
        this._app.MapHealthController();
        this._app.StartAsync().GetAwaiter().GetResult();

        this._client = this._app.GetTestClient();
    }

    public void Dispose()
    {
        this._client.Dispose();
        this._app.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task MapHealthController_RegistersHealthzEndpoint()
    {
        var response = await this._client.GetAsync("/healthz");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", content);
    }

    [Fact]
    public async Task MapHealthController_RegistersVersionEndpoint()
    {
        var response = await this._client.GetAsync("/version");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("version", content);
    }
}
