using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260207;

[UsedImplicitly]
[Migration(202602070830, "Creates tag table with corresponding indexes and keys")]
public class M003CreateTagTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("tag")
            .WithPrimaryId("tag_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("container_name").AsString(128).NotNullable()
            .WithColumn("base_name").AsString(128).NotNullable()
            .WithColumn("parent_name").AsString(128).Nullable()
            .WithColumn("tag_name").AsString(128).NotNullable()
            .WithColumn("tag_depth").AsByte().Nullable()
            .WithColumn("tag_value").AsString(256).Nullable()
            .WithColumn("data_type").AsString(128).Nullable()
            .WithColumn("description").AsString(512).Nullable()
            .WithColumn("external_access").AsString(32).Nullable()
            .WithColumn("constant").AsBoolean().Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("tag")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("container_name").Ascending()
            .OnColumn("tag_name").Ascending()
            .WithOptions().Unique();

        Create.Index()
            .OnTable("tag")
            .OnColumn("tag_name").Ascending()
            .OnColumn("snapshot_id").Ascending();

        Create.Index()
            .OnTable("tag")
            .OnColumn("record_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();
    }
}