using Microsoft.Extensions.Configuration;

namespace TikTakToe.Data;

/// <summary>
/// Configuration values used to build a PostgreSQL connection string.
/// </summary>
public sealed class DatabaseConnectionOptions
{
    /// <summary>
    /// Gets or sets the direct connection string from ConnectionStrings:DefaultConnection.
    /// </summary>
    public string? DefaultConnection { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL host (PGHOST).
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL database name (PGDATABASE).
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL user name (PGUSER).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL password (PGPASSWORD).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL port (PGPORT).
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Creates options from IConfiguration.
    /// </summary>
    /// <param name="configuration">Application configuration source.</param>
    /// <returns>Mapped database connection options.</returns>
    public static DatabaseConnectionOptions FromConfiguration(IConfiguration configuration)
    {
        var options = new DatabaseConnectionOptions
        {
            DefaultConnection = configuration.GetConnectionString("DefaultConnection"),
            Host = configuration["PGHOST"],
            Database = configuration["PGDATABASE"],
            Username = configuration["PGUSER"],
            Password = configuration["PGPASSWORD"],
        };

        var portText = configuration["PGPORT"];
        if (int.TryParse(portText, out var parsedPort) && parsedPort > 0)
        {
            options.Port = parsedPort;
        }

        return options;
    }
}
