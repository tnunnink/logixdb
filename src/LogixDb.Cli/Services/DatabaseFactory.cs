using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using LogixDb.Sqlite;
using LogixDb.SqlServer;

namespace LogixDb.Cli.Services;

/// <summary>
/// Provides a mechanism to resolve LogixDB database instances using the specified
/// SQL connection information. This class uses dependency injection to locate and
/// resolve the appropriate database factory based on the connection provider.
/// Implements the <see cref="ILogixDatabaseFactory"/> interface.
/// </summary>
public class DatabaseFactory(IServiceProvider provider) : ILogixDatabaseFactory
{
    /// <inheritdoc />
    public ILogixDatabase Create(SqlConnectionInfo connection)
    {
        ILogixDatabaseFactory factory = connection.Provider switch
        {
            SqlProvider.Sqlite => new SqliteDatabaseFactory(provider),
            SqlProvider.SqlServer => new SqlServerDatabaseFactory(provider),
            _ => throw new ArgumentOutOfRangeException(nameof(connection), connection.Provider, "")
        };

        return factory.Create(connection);
    }
}