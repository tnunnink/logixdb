namespace LogixDb.Data.SqlServer.Tests.Migrations;

[TestFixture]
public class M20260207Tests : SqlServerTestFixture
{
    [Test]
    public async Task MigrateUp_ToM003_CreatesTagTableWithExpectedColumns()
    {
        await Database.Migrate(202602070830);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("tag");

            await AssertColumnDefinition("tag", "tag_id", "int");
            await AssertColumnDefinition("tag", "snapshot_id", "int");
            await AssertColumnDefinition("tag", "container_name", "nvarchar");
            await AssertColumnDefinition("tag", "tag_name", "nvarchar");
            await AssertColumnDefinition("tag", "base_name", "nvarchar");
            await AssertColumnDefinition("tag", "parent_name", "nvarchar");
            await AssertColumnDefinition("tag", "member_name", "nvarchar");
            await AssertColumnDefinition("tag", "tag_value", "nvarchar");
            await AssertColumnDefinition("tag", "data_type", "nvarchar");
            await AssertColumnDefinition("tag", "description", "nvarchar");
            await AssertColumnDefinition("tag", "external_access", "nvarchar");
            await AssertColumnDefinition("tag", "constant", "bit");
            await AssertColumnDefinition("tag", "record_hash", "varbinary");

            await AssertPrimaryKey("tag", "tag_id");
            await AssertForeignKey("tag", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("tag", "snapshot_id", "container_name", "tag_name");
            await AssertIndex("tag", "tag_name", "snapshot_id");
            await AssertIndex("tag", "record_hash", "snapshot_id");
        }
    }
}