namespace LogixDb.Sqlite.Tests.Migrations;

[TestFixture]
public class M20260212Tests : SqliteMigrationTestBase
{
    [Test]
    public void MigrateUp_ToM011_CreatesAoiTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602120830);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "aoi");

            AssertColumnDefinition(connection, "aoi", "aoi_id", "integer");
            AssertColumnDefinition(connection, "aoi", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "aoi", "name", "text");
            AssertColumnDefinition(connection, "aoi", "revision", "text");
            AssertColumnDefinition(connection, "aoi", "revision_extension", "text");
            AssertColumnDefinition(connection, "aoi", "revision_note", "text");
            AssertColumnDefinition(connection, "aoi", "vendor", "text");
            AssertColumnDefinition(connection, "aoi", "description", "text");
            AssertColumnDefinition(connection, "aoi", "execute_pre_scan", "integer");
            AssertColumnDefinition(connection, "aoi", "execute_post_scan", "integer");
            AssertColumnDefinition(connection, "aoi", "execute_enable_in_false", "integer");
            AssertColumnDefinition(connection, "aoi", "created_date", "datetime");
            AssertColumnDefinition(connection, "aoi", "created_by", "text");
            AssertColumnDefinition(connection, "aoi", "edited_date", "datetime");
            AssertColumnDefinition(connection, "aoi", "edited_by", "text");
            AssertColumnDefinition(connection, "aoi", "software_revision", "text");
            AssertColumnDefinition(connection, "aoi", "help_text", "text");
            AssertColumnDefinition(connection, "aoi", "is_encrypted", "integer");
            AssertColumnDefinition(connection, "aoi", "signature_id", "text");
            AssertColumnDefinition(connection, "aoi", "signature_timestamp", "datetime");
            AssertColumnDefinition(connection, "aoi", "component_class", "text");
            AssertColumnDefinition(connection, "aoi", "record_hash", "text");

            AssertPrimaryKey(connection, "aoi", "aoi_id");
            AssertForeignKey(connection, "aoi", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "aoi", "snapshot_id", "name");
            AssertIndex(connection, "aoi", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public void MigrateUp_ToM012_CreatesAoiParameterTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602120900);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "aoi_parameter");

            AssertColumnDefinition(connection, "aoi_parameter", "parameter_id", "integer");
            AssertColumnDefinition(connection, "aoi_parameter", "aoi_id", "integer");
            AssertColumnDefinition(connection, "aoi_parameter", "name", "text");
            AssertColumnDefinition(connection, "aoi_parameter", "data_type", "text");
            AssertColumnDefinition(connection, "aoi_parameter", "value", "text");
            AssertColumnDefinition(connection, "aoi_parameter", "description", "text");
            AssertColumnDefinition(connection, "aoi_parameter", "external_access", "text");
            AssertColumnDefinition(connection, "aoi_parameter", "tag_usage", "text");
            AssertColumnDefinition(connection, "aoi_parameter", "tag_type", "text");
            AssertColumnDefinition(connection, "aoi_parameter", "alias", "text");
            AssertColumnDefinition(connection, "aoi_parameter", "visible", "integer");
            AssertColumnDefinition(connection, "aoi_parameter", "required", "integer");
            AssertColumnDefinition(connection, "aoi_parameter", "constant", "integer");
            AssertColumnDefinition(connection, "aoi_parameter", "is_atomic", "integer");
            AssertColumnDefinition(connection, "aoi_parameter", "is_array", "integer");
            AssertColumnDefinition(connection, "aoi_parameter", "record_hash", "text");

            AssertPrimaryKey(connection, "aoi_parameter", "parameter_id");
            AssertForeignKey(connection, "aoi_parameter", "aoi_id", "aoi", "aoi_id");
            AssertUniqueIndex(connection, "aoi_parameter", "aoi_id", "name");
        }
    }

    [Test]
    public void MigrateUp_ToM013_CreatesModuleTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602120930);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "module");

            AssertColumnDefinition(connection, "module", "module_id", "integer");
            AssertColumnDefinition(connection, "module", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "module", "name", "text");
            AssertColumnDefinition(connection, "module", "catalog_number", "text");
            AssertColumnDefinition(connection, "module", "revision", "text");
            AssertColumnDefinition(connection, "module", "description", "text");
            AssertColumnDefinition(connection, "module", "vendor_id", "integer");
            AssertColumnDefinition(connection, "module", "product_id", "integer");
            AssertColumnDefinition(connection, "module", "product_code", "integer");
            AssertColumnDefinition(connection, "module", "parent_name", "text");
            AssertColumnDefinition(connection, "module", "parent_port", "integer");
            AssertColumnDefinition(connection, "module", "electronic_keying", "text");
            AssertColumnDefinition(connection, "module", "inhibited", "integer");
            AssertColumnDefinition(connection, "module", "major_fault", "integer");
            AssertColumnDefinition(connection, "module", "safety_enabled", "integer");
            AssertColumnDefinition(connection, "module", "ip_address", "text");
            AssertColumnDefinition(connection, "module", "slot_number", "integer");
            AssertColumnDefinition(connection, "module", "record_hash", "text");

            AssertPrimaryKey(connection, "module", "module_id");
            AssertForeignKey(connection, "module", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "module", "snapshot_id", "name");
            AssertIndex(connection, "module", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public void MigrateUp_ToM014_CreatesCrossReferenceTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602121000);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "cross_reference");

            AssertColumnDefinition(connection, "cross_reference", "reference_id", "integer");
            AssertColumnDefinition(connection, "cross_reference", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "cross_reference", "source_type", "text");
            AssertColumnDefinition(connection, "cross_reference", "source_scope", "text");
            AssertColumnDefinition(connection, "cross_reference", "source_container", "text");
            AssertColumnDefinition(connection, "cross_reference", "source_routine", "text");
            AssertColumnDefinition(connection, "cross_reference", "source_id", "text");
            AssertColumnDefinition(connection, "cross_reference", "source_element", "text");
            AssertColumnDefinition(connection, "cross_reference", "target_type", "text");
            AssertColumnDefinition(connection, "cross_reference", "target_scope", "text");
            AssertColumnDefinition(connection, "cross_reference", "target_container", "text");
            AssertColumnDefinition(connection, "cross_reference", "target_id", "text");

            AssertPrimaryKey(connection, "cross_reference", "reference_id");
            AssertForeignKey(connection, "cross_reference", "snapshot_id", "snapshot", "snapshot_id");
            AssertIndex(connection, "cross_reference", "snapshot_id", "source_type", "source_scope", "source_container", "source_routine", "source_id", "source_element");
            AssertIndex(connection, "cross_reference", "snapshot_id", "target_type", "target_scope", "target_container", "target_id");
        }
    }
}