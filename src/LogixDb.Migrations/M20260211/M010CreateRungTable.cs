using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260211;

[UsedImplicitly]
[Migration(202602111945)]
public class M010CreateRungTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("rung")
            .WithPrimaryId("rung_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("rung_number").AsInt32().NotNullable()
            .WithColumn("scope_type").AsString(32).NotNullable()
            .WithColumn("container_name").AsString(128).NotNullable()
            .WithColumn("routine_name").AsString(128).NotNullable()
            .WithColumn("code").AsString(int.MaxValue).Nullable()
            .WithColumn("comment").AsString(256).Nullable()
            .WithColumn("code_hash").AsString(32).NotNullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("rung")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("container_name").Ascending()
            .OnColumn("routine_name").Ascending()
            .OnColumn("rung_number").Ascending()
            .WithOptions().Unique();

        Create.Index()
            .OnTable("rung")
            .OnColumn("record_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();

        Create.Index()
            .OnTable("rung")
            .OnColumn("code_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();
    }
}