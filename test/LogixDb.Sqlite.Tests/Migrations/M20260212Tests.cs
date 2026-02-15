namespace LogixDb.Sqlite.Tests.Migrations;

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
            await AssertColumnDefinition("aoi", "name", "text");
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
            await AssertColumnDefinition("aoi", "record_hash", "text");

            await AssertPrimaryKey("aoi", "aoi_id");
            await AssertForeignKey("aoi", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("aoi", "snapshot_id", "name");
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
            await AssertColumnDefinition("aoi_parameter", "host_name", "text");
            await AssertColumnDefinition("aoi_parameter", "parameter_name", "text");
            await AssertColumnDefinition("aoi_parameter", "data_type", "text");
            await AssertColumnDefinition("aoi_parameter", "value", "text");
            await AssertColumnDefinition("aoi_parameter", "description", "text");
            await AssertColumnDefinition("aoi_parameter", "external_access", "text");
            await AssertColumnDefinition("aoi_parameter", "tag_usage", "text");
            await AssertColumnDefinition("aoi_parameter", "tag_type", "text");
            await AssertColumnDefinition("aoi_parameter", "alias", "text");
            await AssertColumnDefinition("aoi_parameter", "visible", "integer");
            await AssertColumnDefinition("aoi_parameter", "required", "integer");
            await AssertColumnDefinition("aoi_parameter", "constant", "integer");
            await AssertColumnDefinition("aoi_parameter", "is_atomic", "integer");
            await AssertColumnDefinition("aoi_parameter", "is_array", "integer");
            await AssertColumnDefinition("aoi_parameter", "record_hash", "text");

            await AssertPrimaryKey("aoi_parameter", "parameter_id");
            await AssertForeignKey("aoi_parameter", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("aoi_parameter", "snapshot_id", "host_name", "parameter_name");
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
            await AssertColumnDefinition("module", "name", "text");
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
            await AssertColumnDefinition("module", "record_hash", "text");

            await AssertPrimaryKey("module", "module_id");
            await AssertForeignKey("module", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("module", "snapshot_id", "name");
            await AssertIndex("module", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public async Task MigrateUp_ToM014_CreatesCrossReferenceTableWithExpectedColumns()
    {
        await Database.Migrate(202602121000);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("cross_reference");

            await AssertColumnDefinition("cross_reference", "reference_id", "integer");
            await AssertColumnDefinition("cross_reference", "snapshot_id", "integer");
            await AssertColumnDefinition("cross_reference", "source_type", "text");
            await AssertColumnDefinition("cross_reference", "source_scope", "text");
            await AssertColumnDefinition("cross_reference", "source_container", "text");
            await AssertColumnDefinition("cross_reference", "source_routine", "text");
            await AssertColumnDefinition("cross_reference", "source_id", "text");
            await AssertColumnDefinition("cross_reference", "source_element", "text");
            await AssertColumnDefinition("cross_reference", "target_type", "text");
            await AssertColumnDefinition("cross_reference", "target_scope", "text");
            await AssertColumnDefinition("cross_reference", "target_container", "text");
            await AssertColumnDefinition("cross_reference", "target_id", "text");

            await AssertPrimaryKey("cross_reference", "reference_id");
            await AssertForeignKey("cross_reference", "snapshot_id", "snapshot", "snapshot_id");
            await AssertIndex("cross_reference", "snapshot_id", "source_type", "source_scope", "source_container", "source_routine", "source_id", "source_element");
            await AssertIndex("cross_reference", "snapshot_id", "target_type", "target_scope", "target_container", "target_id");
        }
    }
}