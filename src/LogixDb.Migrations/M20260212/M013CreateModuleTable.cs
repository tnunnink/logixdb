using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260212;

[UsedImplicitly]
[Migration(202602120930, "Creates module table with corresponding indexes and keys")]
public class M013CreateModuleTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("module")
            .WithPrimaryId("module_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("name").AsString(128).NotNullable()
            .WithColumn("catalog_number").AsString(64).Nullable()
            .WithColumn("revision").AsString(16).Nullable()
            .WithColumn("description").AsString(512).Nullable()
            .WithColumn("vendor_id").AsInt32().Nullable()
            .WithColumn("product_id").AsInt32().Nullable()
            .WithColumn("product_code").AsInt16().Nullable()
            .WithColumn("parent_name").AsString(128).Nullable()
            .WithColumn("parent_port").AsByte().Nullable()
            .WithColumn("electronic_keying").AsString(32).Nullable()
            .WithColumn("inhibited").AsBoolean().Nullable()
            .WithColumn("major_fault").AsBoolean().Nullable()
            .WithColumn("safety_enabled").AsBoolean().Nullable()
            .WithColumn("ip_address").AsString(32).Nullable()
            .WithColumn("slot_number").AsByte().Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("module")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("name").Ascending()
            .WithOptions().Unique();

        Create.Index()
            .OnTable("module")
            .OnColumn("record_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();
    }
}