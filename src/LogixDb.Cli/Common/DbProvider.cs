using LogixDb.Data;
using LogixDb.Data.Abstractions;
using LogixDb.Data.Sqlite;
using LogixDb.Data.SqlServer;

namespace LogixDb.Cli.Common;

/// <summary>
/// Provides static methods to create and manage database connections using different SQL providers.
/// </summary>
public static class DbProvider
{
    /// <summary>
    /// Creates an instance of a database connection using the specified SQL provider and connection information.
    /// </summary>
    /// <param name="connection">The connection information that includes the provider type, data source, and optional credentials.</param>
    /// <returns>An instance of <see cref="ILogixDb"/> corresponding to the specified SQL provider.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided SQL provider is not supported.</exception>
    public static ILogixDb GetDatabase(SqlConnectionInfo connection)
    {
        return connection.Provider switch
        {
            SqlProvider.Sqlite => new SqliteDb(connection),
            SqlProvider.SqlServer => new SqlServerDb(connection),
            _ => throw new ArgumentOutOfRangeException(nameof(connection), connection.Provider,
                "Unsupported SQL provider")
        };
    }
}