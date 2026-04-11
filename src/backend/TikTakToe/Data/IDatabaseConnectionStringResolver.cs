namespace TikTakToe.Data;

/// <summary>
/// Resolves the PostgreSQL connection string for the current application configuration.
/// </summary>
public interface IDatabaseConnectionStringResolver
{
    /// <summary>
    /// Resolves a PostgreSQL connection string.
    /// </summary>
    /// <returns>A PostgreSQL connection string.</returns>
    string Resolve();
}
