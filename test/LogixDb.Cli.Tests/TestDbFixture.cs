using LogixDb.Data;
using LogixDb.Data.Abstractions;
using LogixDb.Data.Sqlite;

namespace LogixDb.Cli.Tests;

/// <summary>
/// 
/// </summary>
public abstract class TestDbFixture
{
    protected TestDbFixture()
    {
        DbConnection = Path.Combine(Path.GetTempPath(), $"LogixTest_{Guid.NewGuid():N}.db");
        Database = new SqliteDb(new SqlConnectionInfo(SqlProvider.Sqlite, DbConnection));
    }

    /// <summary>
    /// Gets the file path of the temporary database used during tests.
    /// This property generates a unique file path within the system's temporary directory
    /// and is used to create a testing SQLite database instance. The database file
    /// is cleaned up after the tests using the <see cref="TearDown"/> method to ensure proper disposal.
    /// </summary>
    protected string DbConnection { get; }

    /// <summary>
    /// Provides access to the database instance used during testing.
    /// This property is an implementation of the <see cref="ILogixDb"/> interface and is
    /// initialized with a SQLite database connection. The database is bound to the temporary
    /// file path defined by the <c>TempDb</c> property, ensuring a unique and isolated testing context.
    /// </summary>
    protected ILogixDb Database { get; }

    /// <summary>
    /// Cleans up the temporary database created during tests by removing the
    /// associated file from the file system. This method ensures that no
    /// residual test data is left behind after the test execution.
    /// </summary>
    /// <remarks>
    /// The method performs a best-effort attempt to delete the file at the path
    /// specified by the <see cref="DbConnection"/> property. If the file cannot be
    /// deleted due to any exceptions, those are silently ignored.
    /// </remarks>
    [TearDown]
    protected void TearDown()
    {
        try
        {
            if (File.Exists(DbConnection))
            {
                File.Delete(DbConnection);
            }
        }
        catch (Exception)
        {
            // best effort
        }
    }
}