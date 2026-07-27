namespace TikTakToe.Tests.Data;

using Microsoft.Extensions.Configuration;
using TikTakToe.Data;

public sealed class DatabaseConnectionOptionsTests
{
    [Fact]
    public void FromConfiguration_MapsConnectionStringAndPgSettings()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=direct;Database=tiktaktoe",
            ["PGHOST"] = "localhost",
            ["PGDATABASE"] = "game_db",
            ["PGUSER"] = "postgres",
            ["PGPASSWORD"] = "secret",
            ["PGPORT"] = "5433",
        });

        var options = DatabaseConnectionOptions.FromConfiguration(configuration);

        Assert.Equal("Host=direct;Database=tiktaktoe", options.DefaultConnection);
        Assert.Equal("localhost", options.Host);
        Assert.Equal("game_db", options.Database);
        Assert.Equal("postgres", options.Username);
        Assert.Equal("secret", options.Password);
        Assert.Equal(5433, options.Port);
    }

    [Fact]
    public void FromConfiguration_WhenPortMissing_LeavesPortNull()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["PGHOST"] = "localhost",
            ["PGDATABASE"] = "game_db",
            ["PGUSER"] = "postgres",
        });

        var options = DatabaseConnectionOptions.FromConfiguration(configuration);

        Assert.Null(options.Port);
        Assert.Null(options.DefaultConnection);
        Assert.Null(options.Password);
    }

    [Fact]
    public void FromConfiguration_WhenPortInvalid_LeavesPortNull()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["PGPORT"] = "not-a-number",
        });

        var options = DatabaseConnectionOptions.FromConfiguration(configuration);

        Assert.Null(options.Port);
    }

    [Fact]
    public void FromConfiguration_WhenPortZeroOrNegative_LeavesPortNull()
    {
        var zeroPort = DatabaseConnectionOptions.FromConfiguration(
            BuildConfiguration(new Dictionary<string, string?> { ["PGPORT"] = "0" }));
        var negativePort = DatabaseConnectionOptions.FromConfiguration(
            BuildConfiguration(new Dictionary<string, string?> { ["PGPORT"] = "-1" }));

        Assert.Null(zeroPort.Port);
        Assert.Null(negativePort.Port);
    }

    [Fact]
    public void FromConfiguration_WhenKeysMissing_ReturnsEmptyOptions()
    {
        var options = DatabaseConnectionOptions.FromConfiguration(BuildConfiguration([]));

        Assert.Null(options.DefaultConnection);
        Assert.Null(options.Host);
        Assert.Null(options.Database);
        Assert.Null(options.Username);
        Assert.Null(options.Password);
        Assert.Null(options.Port);
    }

    private static IConfiguration BuildConfiguration(IEnumerable<KeyValuePair<string, string?>> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
