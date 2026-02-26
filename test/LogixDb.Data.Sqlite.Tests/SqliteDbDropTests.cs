using NUnit.Framework.Legacy;

namespace LogixDb.Data.Sqlite.Tests;

[TestFixture]
public class SqliteDbDropTests : SqliteTestFixture
{
    [Test]
    public async Task Drop_WhenCalled_FileShouldNotExist()
    {
        await Database.Migrate();
        FileAssert.Exists(TempDb);

        await Database.Drop();
        FileAssert.DoesNotExist(TempDb);
    }
}