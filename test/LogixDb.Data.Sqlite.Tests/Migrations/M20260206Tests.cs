namespace LogixDb.Data.Sqlite.Tests.Migrations;

[TestFixture]
public class M20260206Tests : SqliteTestFixture
{
    [Test]
    public async Task MigrateUp_ToM001_CreatesTargetTableWithExpectedColumns()
    {
        await Database.Migrate(202602061010);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("target");

            await AssertColumnDefinition("target", "target_id", "integer");
            await AssertColumnDefinition("target", "target_key", "text");
            await AssertColumnDefinition("target", "created_on", "datetime");

            await AssertPrimaryKey("target", "target_id");
            await AssertIndex("target", "target_key");
        }
    }

    [Test]
    public async Task MigrateUp_ToM002_CreatesTargetTableWithExpectedColumns()
    {
        await Database.Migrate(202602061020);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("snapshot");

            await AssertColumnDefinition("snapshot", "snapshot_id", "integer");
            await AssertColumnDefinition("snapshot", "target_id", "integer");
            await AssertColumnDefinition("snapshot", "target_type", "text");
            await AssertColumnDefinition("snapshot", "target_name", "text");
            await AssertColumnDefinition("snapshot", "is_partial", "integer");
            await AssertColumnDefinition("snapshot", "schema_revision", "text");
            await AssertColumnDefinition("snapshot", "software_revision", "text");
            await AssertColumnDefinition("snapshot", "export_date", "datetime");
            await AssertColumnDefinition("snapshot", "export_options", "text");
            await AssertColumnDefinition("snapshot", "import_date", "datetime");
            await AssertColumnDefinition("snapshot", "import_user", "text");
            await AssertColumnDefinition("snapshot", "import_machine", "text");
            await AssertColumnDefinition("snapshot", "source_hash", "blob");
            await AssertColumnDefinition("snapshot", "source_data", "blob");

            await AssertPrimaryKey("snapshot", "snapshot_id");
            await AssertForeignKey("snapshot", "target_id", "target", "target_id");
        }
    }
}