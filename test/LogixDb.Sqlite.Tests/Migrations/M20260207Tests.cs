namespace LogixDb.Sqlite.Tests.Migrations;

[TestFixture]
public class M20260207Tests : SqliteTestFixture
{
    [Test]
    public async Task MigrateUp_ToM003_CreatesTagTableWithExpectedColumns()
    {
        await Database.Migrate(202602070830);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("tag");

            await AssertColumnDefinition("tag", "tag_id", "integer");
            await AssertColumnDefinition("tag", "snapshot_id", "integer");
            await AssertColumnDefinition("tag", "base_name", "text");
            await AssertColumnDefinition("tag", "tag_name", "text");
            await AssertColumnDefinition("tag", "scope_type", "text");
            await AssertColumnDefinition("tag", "container_name", "text");
            await AssertColumnDefinition("tag", "tag_depth", "integer");
            await AssertColumnDefinition("tag", "data_type", "text");
            await AssertColumnDefinition("tag", "value", "text");
            await AssertColumnDefinition("tag", "description", "text");
            await AssertColumnDefinition("tag", "dimensions", "text");
            await AssertColumnDefinition("tag", "external_access", "text");
            await AssertColumnDefinition("tag", "opcua_access", "text");
            await AssertColumnDefinition("tag", "radix", "text");
            await AssertColumnDefinition("tag", "constant", "integer");
            await AssertColumnDefinition("tag", "tag_type", "text");
            await AssertColumnDefinition("tag", "tag_usage", "text");
            await AssertColumnDefinition("tag", "alias", "text");
            await AssertColumnDefinition("tag", "component_class", "text");
            await AssertColumnDefinition("tag", "value_hash", "text");
            await AssertColumnDefinition("tag", "record_hash", "text");

            await AssertPrimaryKey("tag", "tag_id");
            await AssertForeignKey("tag", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("tag", "snapshot_id", "container_name", "tag_name");
        }
    }
}