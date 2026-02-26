using LogixDb.Testing;

namespace LogixDb.Data.Sqlite.Tests;

[TestFixture]
public class SqliteDbPurgeTests : SqliteTestFixture
{
    [Test]
    public async Task Purge_WhenCalled_ShouldRemoveAllSnapshots()
    {
        await Database.Migrate();
        await Database.AddSnapshot(Snapshot.Create(TestSource.LocalTest()));

        await Database.Purge();

        await AssertRecordCount("snapshot", 0);
    }
    
    [Test]
    public async Task Purge_WhenCalled_NotTablesHaveRecords()
    {
        await Database.Migrate();
        await Database.AddSnapshot(Snapshot.Create(TestSource.LocalTest()));

        await Database.Purge();

        await AssertRecordCount("controller", 0);
        await AssertRecordCount("data_type", 0);
        await AssertRecordCount("data_type_member", 0);
        await AssertRecordCount("aoi", 0);
        await AssertRecordCount("aoi_parameter", 0);
        await AssertRecordCount("module", 0);
        await AssertRecordCount("task", 0);
        await AssertRecordCount("program", 0);
        await AssertRecordCount("routine", 0);
        await AssertRecordCount("rung", 0);
        await AssertRecordCount("tag", 0);
    }
}