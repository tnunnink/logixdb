using NUnit.Framework.Legacy;

namespace LogixDb.Data.Sqlite.Tests;

[TestFixture]
public class SqliteDbMigrateTests
{
    /// <summary>
    /// This test is mostly just to refresh a local db instance to inspect and write queries against.
    /// The base test fixture will create and migrate the database
    /// </summary>
    [Test]
    [Explicit("Only run this locally as it is to refresh a database in the project folder")]
    public async Task MigrateLocalTestDatabaseForWritingQueriesAgainst()
    {
        var connection = new SqlConnectionInfo(SqlProvider.Sqlite, "../../../logix.db");
        var database = new SqliteDb(connection);
        await database.Drop();
        await database.Migrate();
        FileAssert.Exists("../../../logix.db");
    }
}