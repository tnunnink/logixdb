using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Sqlite.Tests;

/// <summary>
/// 
/// </summary>
/// <param name="dataSource"></param>
public abstract class SqliteTestFixture(string dataSource)
{
    /// <summary>
    /// The path to the Sqlite database file to create for the test fixture.
    /// </summary>
    protected string DataSource { get; } = dataSource;

    /// <summary>
    /// Builds and initializes an instance of <c>ILogixDatabase</c> using the configured
    /// SQLite data source and dependency injection provider.
    /// </summary>
    /// <returns>
    /// An initialized <c>ILogixDatabase</c> instance configured for the SQLite provider.
    /// </returns>
    protected ILogixDatabase BuildDatabase()
    {
        var provider = new ServiceCollection().AddLogixSqlite().BuildServiceProvider();
        var factory = provider.GetRequiredService<SqliteDatabaseFactory>();
        var connection = new SqlConnectionInfo(SqlProvider.Sqlite, DataSource);
        return factory.Resolve(connection);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (File.Exists(DataSource))
                File.Delete(DataSource);
        }
        catch
        {
            // Best-effort cleanup
        }
    }
}