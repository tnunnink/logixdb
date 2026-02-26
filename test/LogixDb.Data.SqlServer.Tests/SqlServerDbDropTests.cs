namespace LogixDb.Data.SqlServer.Tests;

[TestFixture]
public class SqlServerDbDropTests : SqlServerTestFixture
{
    [Test]
    public async Task Drop_WhenCalled_TablesShouldNotExist()
    {
        await Database.Migrate();
        await AssertTableExists("target");

        await Database.Drop();

        //todo would need an assertion of the database existence
    }
}