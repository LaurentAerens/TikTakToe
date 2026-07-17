namespace TikTakToe.Tests.Data;

using Microsoft.Extensions.Options;
using Npgsql;
using TikTakToe.Data;

public sealed class DatabaseConnectionStringResolverTests
{
    [Fact]
    public void Resolve_WithPgSettings_BuildsConnectionString()
    {
        var resolver = CreateResolver(new DatabaseConnectionOptions
        {
            Host = "db.example",
            Database = "tiktaktoe",
            Username = "app_user",
            Password = "p@ss",
            Port = 5433,
        });

        var connectionString = resolver.Resolve();
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        Assert.Equal("db.example", builder.Host);
        Assert.Equal("tiktaktoe", builder.Database);
        Assert.Equal("app_user", builder.Username);
        Assert.Equal("p@ss", builder.Password);
        Assert.Equal(5433, builder.Port);
    }

    [Fact]
    public void Resolve_WithPgSettingsWithoutPortOrPassword_OmitsOptionalValues()
    {
        var resolver = CreateResolver(new DatabaseConnectionOptions
        {
            Host = "localhost",
            Database = "game_db",
            Username = "postgres",
        });

        var connectionString = resolver.Resolve();
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        Assert.Equal("localhost", builder.Host);
        Assert.Equal("game_db", builder.Database);
        Assert.Equal("postgres", builder.Username);
        Assert.True(string.IsNullOrEmpty(builder.Password));
        Assert.Equal(5432, builder.Port); // Npgsql default when port is omitted
    }

    [Fact]
    public void Resolve_WhenPortIsZero_DoesNotSetPort()
    {
        var resolver = CreateResolver(new DatabaseConnectionOptions
        {
            Host = "localhost",
            Database = "game_db",
            Username = "postgres",
            Port = 0,
        });

        var connectionString = resolver.Resolve();
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        Assert.Equal(5432, builder.Port); // still Npgsql default
    }

    [Fact]
    public void Resolve_WhenPgSettingsIncomplete_FallsBackToDefaultConnection()
    {
        var resolver = CreateResolver(new DatabaseConnectionOptions
        {
            Host = "localhost",
            Database = null,
            Username = "postgres",
            DefaultConnection = "Host=fallback;Database=fallback_db;Username=fallback_user",
        });

        var connectionString = resolver.Resolve();

        Assert.Equal("Host=fallback;Database=fallback_db;Username=fallback_user", connectionString);
    }

    [Fact]
    public void Resolve_WhenOnlyDefaultConnectionConfigured_ReturnsDefaultConnection()
    {
        const string expected = "Host=only-default;Database=db;Username=user";
        var resolver = CreateResolver(new DatabaseConnectionOptions
        {
            DefaultConnection = expected,
        });

        var connectionString = resolver.Resolve();

        Assert.Equal(expected, connectionString);
    }

    [Fact]
    public void Resolve_WhenPgSettingsPresent_PrefersPgSettingsOverDefaultConnection()
    {
        var resolver = CreateResolver(new DatabaseConnectionOptions
        {
            Host = "pg-host",
            Database = "pg-db",
            Username = "pg-user",
            DefaultConnection = "Host=should-not-use;Database=other;Username=other",
        });

        var connectionString = resolver.Resolve();
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        Assert.Equal("pg-host", builder.Host);
        Assert.Equal("pg-db", builder.Database);
        Assert.Equal("pg-user", builder.Username);
    }

    [Fact]
    public void Resolve_WhenNothingConfigured_ThrowsInvalidOperationException()
    {
        var resolver = CreateResolver(new DatabaseConnectionOptions());

        var exception = Assert.Throws<InvalidOperationException>(() => resolver.Resolve());

        Assert.Contains("Database connection is not configured", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ConnectionStrings:DefaultConnection", exception.Message, StringComparison.Ordinal);
        Assert.Contains("PGHOST", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null, "db", "user")]
    [InlineData("host", null, "user")]
    [InlineData("host", "db", null)]
    [InlineData("", "db", "user")]
    [InlineData("host", "   ", "user")]
    [InlineData("host", "db", "\t")]
    public void Resolve_WhenAnyRequiredPgSettingMissing_AndNoDefault_Throws(
        string? host,
        string? database,
        string? username)
    {
        var resolver = CreateResolver(new DatabaseConnectionOptions
        {
            Host = host,
            Database = database,
            Username = username,
        });

        Assert.Throws<InvalidOperationException>(() => resolver.Resolve());
    }

    [Fact]
    public void Resolve_WhenDefaultConnectionIsWhitespace_Throws()
    {
        var resolver = CreateResolver(new DatabaseConnectionOptions
        {
            DefaultConnection = "   ",
        });

        Assert.Throws<InvalidOperationException>(() => resolver.Resolve());
    }

    private static DatabaseConnectionStringResolver CreateResolver(DatabaseConnectionOptions options)
    {
        return new DatabaseConnectionStringResolver(Options.Create(options));
    }
}
