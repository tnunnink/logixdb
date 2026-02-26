namespace LogixDb.Data.Sqlite.Tests.Migrations;

[TestFixture]
public class M20260212Tests : SqliteTestFixture
{
    [Test]
    public async Task MigrateUp_ToM011_CreatesAoiTableWithExpectedColumns()
    {
        await Database.Migrate(202602120830);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("aoi");

            await AssertColumnDefinition("aoi", "aoi_id", "integer");
            await AssertColumnDefinition("aoi", "snapshot_id", "integer");
            await AssertColumnDefinition("aoi", "aoi_name", "text");
            await AssertColumnDefinition("aoi", "revision", "text");
            await AssertColumnDefinition("aoi", "revision_extension", "text");
            await AssertColumnDefinition("aoi", "revision_note", "text");
            await AssertColumnDefinition("aoi", "vendor", "text");
            await AssertColumnDefinition("aoi", "description", "text");
            await AssertColumnDefinition("aoi", "execute_pre_scan", "integer");
            await AssertColumnDefinition("aoi", "execute_post_scan", "integer");
            await AssertColumnDefinition("aoi", "execute_enable_in_false", "integer");
            await AssertColumnDefinition("aoi", "created_date", "datetime");
            await AssertColumnDefinition("aoi", "created_by", "text");
            await AssertColumnDefinition("aoi", "edited_date", "datetime");
            await AssertColumnDefinition("aoi", "edited_by", "text");
            await AssertColumnDefinition("aoi", "software_revision", "text");
            await AssertColumnDefinition("aoi", "help_text", "text");
            await AssertColumnDefinition("aoi", "is_encrypted", "integer");
            await AssertColumnDefinition("aoi", "signature_id", "text");
            await AssertColumnDefinition("aoi", "signature_timestamp", "datetime");
            await AssertColumnDefinition("aoi", "component_class", "text");
            await AssertColumnDefinition("aoi", "record_hash", "blob");

            await AssertPrimaryKey("aoi", "aoi_id");
            await AssertForeignKey("aoi", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("aoi", "snapshot_id", "aoi_name");
            await AssertIndex("aoi", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public async Task MigrateUp_ToM012_CreatesAoiParameterTableWithExpectedColumns()
    {
        await Database.Migrate(202602120900);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("aoi_parameter");

            await AssertColumnDefinition("aoi_parameter", "parameter_id", "integer");
            await AssertColumnDefinition("aoi_parameter", "snapshot_id", "integer");
            await AssertColumnDefinition("aoi_parameter", "aoi_name", "text");
            await AssertColumnDefinition("aoi_parameter", "parameter_name", "text");
            await AssertColumnDefinition("aoi_parameter", "data_type", "text");
            await AssertColumnDefinition("aoi_parameter", "default_value", "text");
            await AssertColumnDefinition("aoi_parameter", "description", "text");
            await AssertColumnDefinition("aoi_parameter", "external_access", "text");
            await AssertColumnDefinition("aoi_parameter", "tag_usage", "text");
            await AssertColumnDefinition("aoi_parameter", "tag_type", "text");
            await AssertColumnDefinition("aoi_parameter", "tag_alias", "text");
            await AssertColumnDefinition("aoi_parameter", "visible", "integer");
            await AssertColumnDefinition("aoi_parameter", "required", "integer");
            await AssertColumnDefinition("aoi_parameter", "constant", "integer");
            await AssertColumnDefinition("aoi_parameter", "record_hash", "blob");

            await AssertPrimaryKey("aoi_parameter", "parameter_id");
            await AssertForeignKey("aoi_parameter", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("aoi_parameter", "snapshot_id", "aoi_name", "parameter_name");
        }
    }

    [Test]
    public async Task MigrateUp_ToM013_CreatesModuleTableWithExpectedColumns()
    {
        await Database.Migrate(202602120930);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("module");

            await AssertColumnDefinition("module", "module_id", "integer");
            await AssertColumnDefinition("module", "snapshot_id", "integer");
            await AssertColumnDefinition("module", "module_name", "text");
            await AssertColumnDefinition("module", "catalog_number", "text");
            await AssertColumnDefinition("module", "revision", "text");
            await AssertColumnDefinition("module", "description", "text");
            await AssertColumnDefinition("module", "vendor_id", "integer");
            await AssertColumnDefinition("module", "product_id", "integer");
            await AssertColumnDefinition("module", "product_code", "integer");
            await AssertColumnDefinition("module", "parent_name", "text");
            await AssertColumnDefinition("module", "parent_port", "integer");
            await AssertColumnDefinition("module", "electronic_keying", "text");
            await AssertColumnDefinition("module", "inhibited", "integer");
            await AssertColumnDefinition("module", "major_fault", "integer");
            await AssertColumnDefinition("module", "safety_enabled", "integer");
            await AssertColumnDefinition("module", "ip_address", "text");
            await AssertColumnDefinition("module", "slot_number", "integer");
            await AssertColumnDefinition("module", "record_hash", "blob");

            await AssertPrimaryKey("module", "module_id");
            await AssertForeignKey("module", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("module", "snapshot_id", "module_name");
            await AssertIndex("module", "record_hash", "snapshot_id");
        }
    }
}