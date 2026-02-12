namespace LogixDb.Sqlite.Tests.Migrations;

[TestFixture]
public class M20260211Tests : SqliteMigrationTestBase
{
    [Test]
    public void MigrateUp_ToM004_CreatesControllerTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602111430);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "controller");

            AssertColumnDefinition(connection, "controller", "controller_id", "integer");
            AssertColumnDefinition(connection, "controller", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "controller", "name", "text");
            AssertColumnDefinition(connection, "controller", "processor", "text");
            AssertColumnDefinition(connection, "controller", "revision", "text");
            AssertColumnDefinition(connection, "controller", "description", "text");
            AssertColumnDefinition(connection, "controller", "project_creation_date", "datetime");
            AssertColumnDefinition(connection, "controller", "last_modified_date", "datetime");
            AssertColumnDefinition(connection, "controller", "comm_path", "text");
            AssertColumnDefinition(connection, "controller", "sfc_execution_control", "text");
            AssertColumnDefinition(connection, "controller", "sfc_restart_position", "text");
            AssertColumnDefinition(connection, "controller", "sfc_last_scan", "text");
            AssertColumnDefinition(connection, "controller", "project_sn", "text");
            AssertColumnDefinition(connection, "controller", "match_project_to_controller", "integer");
            AssertColumnDefinition(connection, "controller", "inhibit_firmware_updates", "integer");
            AssertColumnDefinition(connection, "controller", "allow_rfi_from_producer", "integer");
            AssertColumnDefinition(connection, "controller", "pass_through", "text");
            AssertColumnDefinition(connection, "controller", "download_documentation", "integer");
            AssertColumnDefinition(connection, "controller", "download_properties", "integer");
            AssertColumnDefinition(connection, "controller", "ethernet_ip_mode", "text");
            AssertColumnDefinition(connection, "controller", "record_hash", "text");

            AssertPrimaryKey(connection, "controller", "controller_id");
            AssertForeignKey(connection, "controller", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "controller", "name", "snapshot_id");
        }
    }

    [Test]
    public void MigrateUp_ToM005_CreatesDataTypeTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602111500);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "data_type");

            AssertColumnDefinition(connection, "data_type", "type_id", "integer");
            AssertColumnDefinition(connection, "data_type", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "data_type", "name", "text");
            AssertColumnDefinition(connection, "data_type", "type_class", "text");
            AssertColumnDefinition(connection, "data_type", "type_family", "text");
            AssertColumnDefinition(connection, "data_type", "description", "text");
            AssertColumnDefinition(connection, "data_type", "record_hash", "text");

            AssertPrimaryKey(connection, "data_type", "type_id");
            AssertForeignKey(connection, "data_type", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "data_type", "snapshot_id", "name");
            AssertIndex(connection, "data_type", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public void MigrateUp_ToM006_CreatesDataTypeMemberTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602111530);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "data_type_member");

            AssertColumnDefinition(connection, "data_type_member", "member_id", "integer");
            AssertColumnDefinition(connection, "data_type_member", "type_id", "integer");
            AssertColumnDefinition(connection, "data_type_member", "name", "text");
            AssertColumnDefinition(connection, "data_type_member", "data_type", "text");
            AssertColumnDefinition(connection, "data_type_member", "dimension", "integer");
            AssertColumnDefinition(connection, "data_type_member", "radix", "text");
            AssertColumnDefinition(connection, "data_type_member", "external_access", "text");
            AssertColumnDefinition(connection, "data_type_member", "description", "text");
            AssertColumnDefinition(connection, "data_type_member", "hidden", "integer");
            AssertColumnDefinition(connection, "data_type_member", "target", "text");
            AssertColumnDefinition(connection, "data_type_member", "bit_number", "integer");
            AssertColumnDefinition(connection, "data_type_member", "record_hash", "text");

            AssertPrimaryKey(connection, "data_type_member", "member_id");
            AssertForeignKey(connection, "data_type_member", "type_id", "data_type", "type_id");
            AssertUniqueIndex(connection, "data_type_member", "type_id", "name");
        }
    }

    [Test]
    public void MigrateUp_ToM007_CreatesTaskTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602111600);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "task");

            AssertColumnDefinition(connection, "task", "task_id", "integer");
            AssertColumnDefinition(connection, "task", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "task", "name", "text");
            AssertColumnDefinition(connection, "task", "task_type", "text");
            AssertColumnDefinition(connection, "task", "description", "text");
            AssertColumnDefinition(connection, "task", "priority", "integer");
            AssertColumnDefinition(connection, "task", "rate", "numeric");
            AssertColumnDefinition(connection, "task", "watchdog", "numeric");
            AssertColumnDefinition(connection, "task", "inhibited", "integer");
            AssertColumnDefinition(connection, "task", "disable_outputs", "integer");
            AssertColumnDefinition(connection, "task", "event_trigger", "text");
            AssertColumnDefinition(connection, "task", "event_tag", "text");
            AssertColumnDefinition(connection, "task", "enable_timeout", "integer");
            AssertColumnDefinition(connection, "task", "record_hash", "text");

            AssertPrimaryKey(connection, "task", "task_id");
            AssertForeignKey(connection, "task", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "task", "snapshot_id", "name");
            AssertIndex(connection, "task", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public void MigrateUp_ToM008_CreatesProgramTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602111630);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "program");

            AssertColumnDefinition(connection, "program", "program_id", "integer");
            AssertColumnDefinition(connection, "program", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "program", "name", "text");
            AssertColumnDefinition(connection, "program", "program_type", "text");
            AssertColumnDefinition(connection, "program", "description", "text");
            AssertColumnDefinition(connection, "program", "main_routine", "text");
            AssertColumnDefinition(connection, "program", "fault_routine", "text");
            AssertColumnDefinition(connection, "program", "is_disabled", "integer");
            AssertColumnDefinition(connection, "program", "is_folder", "integer");
            AssertColumnDefinition(connection, "program", "has_test_edits", "integer");
            AssertColumnDefinition(connection, "program", "parent_name", "text");
            AssertColumnDefinition(connection, "program", "task_name", "text");
            AssertColumnDefinition(connection, "program", "record_hash", "text");

            AssertPrimaryKey(connection, "program", "program_id");
            AssertForeignKey(connection, "program", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "program", "snapshot_id", "name");
            AssertIndex(connection, "program", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public void MigrateUp_ToM009_CreatesRoutineTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602111930);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "routine");

            AssertColumnDefinition(connection, "routine", "routine_id", "integer");
            AssertColumnDefinition(connection, "routine", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "routine", "name", "text");
            AssertColumnDefinition(connection, "routine", "scope_type", "text");
            AssertColumnDefinition(connection, "routine", "container_name", "text");
            AssertColumnDefinition(connection, "routine", "routine_type", "text");
            AssertColumnDefinition(connection, "routine", "description", "text");
            AssertColumnDefinition(connection, "routine", "record_hash", "text");

            AssertPrimaryKey(connection, "routine", "routine_id");
            AssertForeignKey(connection, "routine", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "routine", "snapshot_id", "container_name", "name", "scope_type");
            AssertIndex(connection, "routine", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public void MigrateUp_ToM010_CreatesRungTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602111945);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "rung");

            AssertColumnDefinition(connection, "rung", "rung_id", "integer");
            AssertColumnDefinition(connection, "rung", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "rung", "rung_number", "integer");
            AssertColumnDefinition(connection, "rung", "scope_type", "text");
            AssertColumnDefinition(connection, "rung", "container_name", "text");
            AssertColumnDefinition(connection, "rung", "routine_name", "text");
            AssertColumnDefinition(connection, "rung", "code", "text");
            AssertColumnDefinition(connection, "rung", "comment", "text");
            AssertColumnDefinition(connection, "rung", "code_hash", "text");
            AssertColumnDefinition(connection, "rung", "record_hash", "text");

            AssertPrimaryKey(connection, "rung", "rung_id");
            AssertForeignKey(connection, "rung", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "rung", "snapshot_id", "container_name", "routine_name", "rung_number");
            AssertIndex(connection, "rung", "record_hash", "snapshot_id");
            AssertIndex(connection, "rung", "code_hash", "snapshot_id");
        }
    }
}