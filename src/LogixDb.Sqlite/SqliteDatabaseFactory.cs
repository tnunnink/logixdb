using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Sqlite;

/// <summary>
/// A factory implementation for creating SQLite database instances conforming to the <see cref="ILogixDatabaseFactory"/> interface.
/// This class simplifies the creation and initialization of SQLite databases while abstracting the underlying details.
/// </summary>
public class SqliteDatabaseFactory(IServiceProvider provider) : ILogixDatabaseFactory
{
    /// <inheritdoc />
    public ILogixDatabase Resolve(SqlConnectionInfo connection)
    {
        var imports = provider.GetServices<ILogixDatabaseImport>();
        return new SqliteDatabase(connection, imports);
    }
}