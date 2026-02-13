using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260206;

[UsedImplicitly]
[Migration(202602061020, "Create snapshot table and associated indexes for target type/name and import date")]
public class M002CreateSnapshotTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("snapshot")
            .WithPrimaryId("snapshot_id")
            .WithCascadeForeignKey("target_id", "target")
            .WithColumn("target_type").AsString(128).NotNullable()
            .WithColumn("target_name").AsString(128).NotNullable()
            .WithColumn("is_partial").AsBoolean().NotNullable()
            .WithColumn("schema_revision").AsString(16).Nullable()
            .WithColumn("software_revision").AsString(16).Nullable()
            .WithColumn("export_date").AsDateTime().Nullable()
            .WithColumn("export_options").AsString(256).Nullable()
            .WithColumn("import_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("import_user").AsString(64).NotNullable().WithDefaultValue(Environment.UserName)
            .WithColumn("import_machine").AsString(64).NotNullable().WithDefaultValue(Environment.MachineName)
            .WithColumn("source_hash").AsString(64).Nullable()
            .WithColumn("source_data").AsBinary(int.MaxValue).Nullable();

        Create.Index()
            .OnTable("snapshot")
            .OnColumn("target_type").Ascending()
            .OnColumn("target_name").Ascending();

        Create.Index()
            .OnTable("snapshot")
            .OnColumn("target_id").Ascending()
            .OnColumn("import_date").Descending();
    }
}