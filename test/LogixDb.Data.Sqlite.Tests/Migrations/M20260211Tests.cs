namespace LogixDb.Data.Sqlite.Tests.Migrations;

[TestFixture]
public class M20260211Tests : SqliteTestFixture
{
    [Test]
    public async Task MigrateUp_ToM004_CreatesControllerTableWithExpectedColumns()
    {
        await Database.Migrate(202602111430);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("controller");

            await AssertColumnDefinition("controller", "controller_id", "integer");
            await AssertColumnDefinition("controller", "snapshot_id", "integer");
            await AssertColumnDefinition("controller", "controller_name", "text");
            await AssertColumnDefinition("controller", "processor", "text");
            await AssertColumnDefinition("controller", "revision", "text");
            await AssertColumnDefinition("controller", "description", "text");
            await AssertColumnDefinition("controller", "project_creation_date", "datetime");
            await AssertColumnDefinition("controller", "last_modified_date", "datetime");
            await AssertColumnDefinition("controller", "comm_path", "text");
            await AssertColumnDefinition("controller", "sfc_execution_control", "text");
            await AssertColumnDefinition("controller", "sfc_restart_position", "text");
            await AssertColumnDefinition("controller", "sfc_last_scan", "text");
            await AssertColumnDefinition("controller", "project_sn", "text");
            await AssertColumnDefinition("controller", "match_project_to_controller", "integer");
            await AssertColumnDefinition("controller", "inhibit_firmware_updates", "integer");
            await AssertColumnDefinition("controller", "allow_rfi_from_producer", "integer");
            await AssertColumnDefinition("controller", "pass_through", "text");
            await AssertColumnDefinition("controller", "download_documentation", "integer");
            await AssertColumnDefinition("controller", "download_properties", "integer");
            await AssertColumnDefinition("controller", "ethernet_ip_mode", "text");
            await AssertColumnDefinition("controller", "record_hash", "blob");

            await AssertPrimaryKey("controller", "controller_id");
            await AssertForeignKey("controller", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("controller", "controller_name", "snapshot_id");
        }
    }

    [Test]
    public async Task MigrateUp_ToM005_CreatesDataTypeTableWithExpectedColumns()
    {
        await Database.Migrate(202602111500);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("data_type");

            await AssertColumnDefinition("data_type", "type_id", "integer");
            await AssertColumnDefinition("data_type", "snapshot_id", "integer");
            await AssertColumnDefinition("data_type", "type_name", "text");
            await AssertColumnDefinition("data_type", "type_class", "text");
            await AssertColumnDefinition("data_type", "type_family", "text");
            await AssertColumnDefinition("data_type", "description", "text");
            await AssertColumnDefinition("data_type", "record_hash", "blob");

            await AssertPrimaryKey("data_type", "type_id");
            await AssertForeignKey("data_type", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("data_type", "snapshot_id", "type_name");
            await AssertIndex("data_type", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public async Task MigrateUp_ToM006_CreatesDataTypeMemberTableWithExpectedColumns()
    {
        await Database.Migrate(202602111530);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("data_type_member");

            await AssertColumnDefinition("data_type_member", "member_id", "integer");
            await AssertColumnDefinition("data_type_member", "snapshot_id", "integer");
            await AssertColumnDefinition("data_type_member", "type_name", "text");
            await AssertColumnDefinition("data_type_member", "member_name", "text");
            await AssertColumnDefinition("data_type_member", "data_type", "text");
            await AssertColumnDefinition("data_type_member", "dimension", "integer");
            await AssertColumnDefinition("data_type_member", "radix", "text");
            await AssertColumnDefinition("data_type_member", "external_access", "text");
            await AssertColumnDefinition("data_type_member", "description", "text");
            await AssertColumnDefinition("data_type_member", "hidden", "integer");
            await AssertColumnDefinition("data_type_member", "target", "text");
            await AssertColumnDefinition("data_type_member", "bit_number", "integer");
            await AssertColumnDefinition("data_type_member", "record_hash", "blob");

            await AssertPrimaryKey("data_type_member", "member_id");
            await AssertForeignKey("data_type_member", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("data_type_member", "snapshot_id", "type_name", "member_name");
        }
    }

    [Test]
    public async Task MigrateUp_ToM007_CreatesTaskTableWithExpectedColumns()
    {
        await Database.Migrate(202602111600);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("task");

            await AssertColumnDefinition("task", "task_id", "integer");
            await AssertColumnDefinition("task", "snapshot_id", "integer");
            await AssertColumnDefinition("task", "task_name", "text");
            await AssertColumnDefinition("task", "task_type", "text");
            await AssertColumnDefinition("task", "description", "text");
            await AssertColumnDefinition("task", "priority", "integer");
            await AssertColumnDefinition("task", "rate", "numeric");
            await AssertColumnDefinition("task", "watchdog", "numeric");
            await AssertColumnDefinition("task", "inhibited", "integer");
            await AssertColumnDefinition("task", "disable_outputs", "integer");
            await AssertColumnDefinition("task", "event_trigger", "text");
            await AssertColumnDefinition("task", "event_tag", "text");
            await AssertColumnDefinition("task", "enable_timeout", "integer");
            await AssertColumnDefinition("task", "record_hash", "blob");

            await AssertPrimaryKey("task", "task_id");
            await AssertForeignKey("task", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("task", "snapshot_id", "task_name");
            await AssertIndex("task", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public async Task MigrateUp_ToM008_CreatesProgramTableWithExpectedColumns()
    {
        await Database.Migrate(202602111630);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("program");

            await AssertColumnDefinition("program", "program_id", "integer");
            await AssertColumnDefinition("program", "snapshot_id", "integer");
            await AssertColumnDefinition("program", "program_name", "text");
            await AssertColumnDefinition("program", "program_type", "text");
            await AssertColumnDefinition("program", "description", "text");
            await AssertColumnDefinition("program", "main_routine", "text");
            await AssertColumnDefinition("program", "fault_routine", "text");
            await AssertColumnDefinition("program", "is_disabled", "integer");
            await AssertColumnDefinition("program", "is_folder", "integer");
            await AssertColumnDefinition("program", "has_test_edits", "integer");
            await AssertColumnDefinition("program", "parent_name", "text");
            await AssertColumnDefinition("program", "task_name", "text");
            await AssertColumnDefinition("program", "record_hash", "blob");

            await AssertPrimaryKey("program", "program_id");
            await AssertForeignKey("program", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("program", "snapshot_id", "program_name");
            await AssertIndex("program", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public async Task MigrateUp_ToM009_CreatesRoutineTableWithExpectedColumns()
    {
        await Database.Migrate(202602111930);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("routine");

            await AssertColumnDefinition("routine", "routine_id", "integer");
            await AssertColumnDefinition("routine", "snapshot_id", "integer");
            await AssertColumnDefinition("routine", "container_name", "text");
            await AssertColumnDefinition("routine", "routine_name", "text");
            await AssertColumnDefinition("routine", "routine_type", "text");
            await AssertColumnDefinition("routine", "description", "text");
            await AssertColumnDefinition("routine", "record_hash", "blob");

            await AssertPrimaryKey("routine", "routine_id");
            await AssertForeignKey("routine", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("routine", "snapshot_id", "container_name", "routine_name");
            await AssertIndex("routine", "record_hash", "snapshot_id");
        }
    }

    [Test]
    public async Task MigrateUp_ToM010_CreatesRungTableWithExpectedColumns()
    {
        await Database.Migrate(202602111945);

        using (Assert.EnterMultipleScope())
        {
            await AssertTableExists("rung");

            await AssertColumnDefinition("rung", "rung_id", "integer");
            await AssertColumnDefinition("rung", "snapshot_id", "integer");
            await AssertColumnDefinition("rung", "container_name", "text");
            await AssertColumnDefinition("rung", "routine_name", "text");
            await AssertColumnDefinition("rung", "rung_number", "integer");
            await AssertColumnDefinition("rung", "code", "text");
            await AssertColumnDefinition("rung", "comment", "text");
            await AssertColumnDefinition("rung", "code_hash", "blob");
            await AssertColumnDefinition("rung", "record_hash", "blob");

            await AssertPrimaryKey("rung", "rung_id");
            await AssertForeignKey("rung", "snapshot_id", "snapshot", "snapshot_id");
            await AssertUniqueIndex("rung", "snapshot_id", "container_name", "routine_name", "rung_number");
            await AssertIndex("rung", "record_hash", "snapshot_id");
            await AssertIndex("rung", "code_hash", "snapshot_id");
        }
    }
}