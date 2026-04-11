using Microsoft.Extensions.Options;
using Npgsql;

namespace TikTakToe.Data;

/// <summary>
/// Resolves PostgreSQL connection details from application configuration.
/// </summary>
public sealed class DatabaseConnectionStringResolver(IOptions<DatabaseConnectionOptions> options) : IDatabaseConnectionStringResolver
{
    /// <summary>
    /// Resolves the connection string using either PG* settings or ConnectionStrings:DefaultConnection.
    /// </summary>
    /// <returns>A PostgreSQL connection string.</returns>
    public string Resolve()
    {
        var configuredOptions = options.Value;
        var host = configuredOptions.Host;
        var database = configuredOptions.Database;
        var username = configuredOptions.Username;

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(database) && !string.IsNullOrWhiteSpace(username))
        {
            var envBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Database = database,
                Username = username,
            };

            if (configuredOptions.Port is > 0)
            {
                envBuilder.Port = configuredOptions.Port.Value;
            }

            if (!string.IsNullOrWhiteSpace(configuredOptions.Password))
            {
                envBuilder.Password = configuredOptions.Password;
            }

            return envBuilder.ToString();
        }

        var directConnection = configuredOptions.DefaultConnection;
        if (!string.IsNullOrWhiteSpace(directConnection))
        {
            return directConnection;
        }

        throw new InvalidOperationException(
            "Database connection is not configured. Set ConnectionStrings:DefaultConnection or PGHOST/PGDATABASE/PGUSER environment variables.");
    }
}