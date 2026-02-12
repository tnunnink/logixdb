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
            .WithColumn("base_name").AsString(128).NotNullable()
            .WithColumn("tag_name").AsString(128).NotNullable()
            .WithColumn("tag_depth").AsByte().NotNullable()
            .WithColumn("scope_type").AsString(32).NotNullable()
            .WithColumn("container_name").AsString(128).NotNullable()
            .WithColumn("data_type").AsString(128).Nullable()
            .WithColumn("value").AsString(256).Nullable()
            .WithColumn("description").AsString(256).Nullable()
            .WithColumn("dimensions").AsString(32).Nullable()
            .WithColumn("external_access").AsString(32).Nullable()
            .WithColumn("opcua_access").AsString(32).Nullable()
            .WithColumn("radix").AsString(32).Nullable()
            .WithColumn("constant").AsBoolean().Nullable()
            .WithColumn("tag_type").AsString(32).Nullable()
            .WithColumn("tag_usage").AsString(32).Nullable()
            .WithColumn("alias_for").AsString(128).Nullable()
            .WithColumn("component_class").AsString(32).Nullable()
            .WithColumn("value_hash").AsString(32).NotNullable()
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