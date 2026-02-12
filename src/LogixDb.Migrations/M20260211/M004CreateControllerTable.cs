using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260211;

[UsedImplicitly]
[Migration(202602111430, "Creates controller table with required keys")]
public class M004CreateControllerTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("controller")
            .WithPrimaryId("controller_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("name").AsString(128).NotNullable()
            .WithColumn("processor").AsString(128).Nullable()
            .WithColumn("revision").AsString(32).Nullable()
            .WithColumn("description").AsString(256).Nullable()
            .WithColumn("project_creation_date").AsDateTime().Nullable()
            .WithColumn("last_modified_date").AsDateTime().Nullable()
            .WithColumn("comm_path").AsString(128).Nullable()
            .WithColumn("sfc_execution_control").AsString(32).Nullable()
            .WithColumn("sfc_restart_position").AsString(32).Nullable()
            .WithColumn("sfc_last_scan").AsString(32).Nullable()
            .WithColumn("project_sn").AsString(32).Nullable()
            .WithColumn("match_project_to_controller").AsBoolean().Nullable()
            .WithColumn("inhibit_firmware_updates").AsBoolean().Nullable()
            .WithColumn("allow_rfi_from_producer").AsBoolean().Nullable()
            .WithColumn("pass_through").AsString(32).Nullable()
            .WithColumn("download_documentation").AsBoolean().Nullable()
            .WithColumn("download_properties").AsBoolean().Nullable()
            .WithColumn("ethernet_ip_mode").AsString(32).Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("controller")
            .OnColumn("name").Ascending()
            .OnColumn("snapshot_id").Ascending()
            .WithOptions().Unique();
    }
}