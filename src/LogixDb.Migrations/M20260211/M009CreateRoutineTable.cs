using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260211;

[UsedImplicitly]
[Migration(202602111930, "Creates routine table with corresponding indexes and keys")]
public class M009CreateRoutineTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("routine")
            .WithPrimaryId("routine_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("name").AsString(128).NotNullable()
            .WithColumn("scope_type").AsString(32).NotNullable()
            .WithColumn("container_name").AsString(128).NotNullable()
            .WithColumn("routine_type").AsString(32).Nullable()
            .WithColumn("description").AsString(512).Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("routine")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("container_name").Ascending()
            .OnColumn("name").Ascending()
            .OnColumn("scope_type").Ascending()
            .WithOptions().Unique();

        Create.Index()
            .OnTable("routine")
            .OnColumn("record_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();
    }
}