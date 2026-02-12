using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260211;

[UsedImplicitly]
[Migration(202602111600, "")]
public class M007CreateTaskTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("task")
            .WithPrimaryId("task_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("name").AsString(128).NotNullable()
            .WithColumn("task_type").AsString(32).Nullable()
            .WithColumn("description").AsString(256).Nullable()
            .WithColumn("priority").AsByte().Nullable()
            .WithColumn("rate").AsFloat().Nullable()
            .WithColumn("watchdog").AsFloat().Nullable()
            .WithColumn("inhibited").AsBoolean().Nullable()
            .WithColumn("disable_outputs").AsBoolean().Nullable()
            .WithColumn("event_trigger").AsString(32).Nullable()
            .WithColumn("event_tag").AsString(128).Nullable()
            .WithColumn("enable_timeout").AsBoolean().Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("task")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("name").Ascending()
            .WithOptions().Unique();
        
        Create.Index()
            .OnTable("task")
            .OnColumn("record_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();
    }
}