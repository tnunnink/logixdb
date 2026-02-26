namespace LogixDb.Data.SqlServer.Tests;

[TestFixture]
public class SqlServerMigrationTest
{
    [Test]
    [Explicit("Manually run against local test server to check migrations and develop SQL queries against")]
    public async Task Build_WithLocalContainerServer_ShouldCreateAndMigrateDatabase()
    {
        var connectionInfo = new SqlConnectionInfo(
            Provider: SqlProvider.SqlServer,
            Source: "localhost,1433",
            Database: "logixdb",
            User: "sa",
            Password: "LogixDb!Test123",
            Trust: true
        );

        var database = new SqlServerDb(connectionInfo);

        await database.Migrate();
    }
}