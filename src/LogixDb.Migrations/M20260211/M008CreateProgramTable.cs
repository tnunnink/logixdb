using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260211;

[UsedImplicitly]
[Migration(202602111630, "Creates program table with corresponding indexes and keys")]
public class M008CreateProgramTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("program")
            .WithPrimaryId("program_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("name").AsString(128).NotNullable()
            .WithColumn("program_type").AsString(32).Nullable()
            .WithColumn("description").AsString(512).Nullable()
            .WithColumn("main_routine").AsString(64).Nullable()
            .WithColumn("fault_routine").AsString(64).Nullable()
            .WithColumn("is_disabled").AsBoolean().Nullable()
            .WithColumn("is_folder").AsBoolean().Nullable()
            .WithColumn("has_test_edits").AsBoolean().Nullable()
            .WithColumn("parent_name").AsString(128).Nullable()
            .WithColumn("task_name").AsString(128).Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("program")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("name").Ascending()
            .WithOptions().Unique();
        
        Create.Index()
            .OnTable("program")
            .OnColumn("record_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();
    }
}