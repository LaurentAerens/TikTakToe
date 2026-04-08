using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TikTakToe.Data;

/// <summary>
/// Resolves PostgreSQL connection details from application configuration.
/// </summary>
public static class DatabaseConnectionStringResolver
{
    /// <summary>
    /// Resolves the connection string using either ConnectionStrings:DefaultConnection
    /// or PG* environment settings.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>A PostgreSQL connection string.</returns>
    public static string Resolve(IConfiguration configuration)
    {
        var host = configuration["PGHOST"];
        var database = configuration["PGDATABASE"];
        var username = configuration["PGUSER"];

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(database) && !string.IsNullOrWhiteSpace(username))
        {
            var portText = configuration["PGPORT"];
            var password = configuration["PGPASSWORD"];

            var envBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Database = database,
                Username = username,
            };

            if (int.TryParse(portText, out var parsedPort) && parsedPort > 0)
            {
                envBuilder.Port = parsedPort;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                envBuilder.Password = password;
            }

            return envBuilder.ToString();
        }

        var directConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(directConnection))
        {
            return directConnection;
        }

        throw new InvalidOperationException(
            "Database connection is not configured. Set ConnectionStrings:DefaultConnection or PGHOST/PGDATABASE/PGUSER environment variables.");
    }
}