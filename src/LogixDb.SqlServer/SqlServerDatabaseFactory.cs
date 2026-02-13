using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.SqlServer;

/// <summary>
/// Factory class responsible for creating instances of SqlServerDatabase.
/// This class implements the ILogixDatabaseFactory interface and provides
/// a mechanism to initialize SQL Server database connections using the
/// provided connection information and associated imports.
/// </summary>
public class SqlServerDatabaseFactory(IServiceProvider provider) : ILogixDatabaseFactory
{
    /// <inheritdoc />
    public ILogixDatabase Create(SqlConnectionInfo connection)
    {
        var imports = provider.GetServices<ILogixDatabaseImport>();
        return new SqlServerDatabase(connection, imports);
    }
}