using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260211;

[UsedImplicitly]
[Migration(202602111530, "Creates data_type_member table with corresponding indexes and keys")]
public class M006CreateDataTypeMemberTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("data_type_member")
            .WithPrimaryId("member_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("host_name").AsString(128).NotNullable()
            .WithColumn("member_name").AsString(128).NotNullable()
            .WithColumn("data_type").AsString(128).Nullable()
            .WithColumn("dimension").AsInt16().Nullable()
            .WithColumn("radix").AsString(32).Nullable()
            .WithColumn("external_access").AsString(32).Nullable()
            .WithColumn("description").AsString(512).Nullable()
            .WithColumn("hidden").AsBoolean().Nullable()
            .WithColumn("target").AsString(64).Nullable()
            .WithColumn("bit_number").AsByte().Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("data_type_member")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("host_name").Ascending()
            .OnColumn("member_name").Ascending()
            .WithOptions().Unique();
    }
}