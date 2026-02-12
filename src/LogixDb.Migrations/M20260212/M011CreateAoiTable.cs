using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260212;

[UsedImplicitly]
[Migration(202602120830, "Creates aoi table with corresponding indexes and keys")]
public class M011CreateAoiTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("aoi")
            .WithPrimaryId("aoi_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("name").AsString(128).NotNullable()
            .WithColumn("revision").AsString(16).Nullable()
            .WithColumn("revision_extension").AsString(64).Nullable()
            .WithColumn("revision_note").AsString(512).Nullable()
            .WithColumn("vendor").AsString(64).Nullable()
            .WithColumn("description").AsString(512).Nullable()
            .WithColumn("execute_pre_scan").AsBoolean().Nullable()
            .WithColumn("execute_post_scan").AsBoolean().Nullable()
            .WithColumn("execute_enable_in_false").AsBoolean().Nullable()
            .WithColumn("created_date").AsDateTime().Nullable()
            .WithColumn("created_by").AsString(64).Nullable()
            .WithColumn("edited_date").AsDateTime().Nullable()
            .WithColumn("edited_by").AsString(64).Nullable()
            .WithColumn("software_revision").AsString(16).Nullable()
            .WithColumn("help_text").AsString(int.MaxValue).Nullable()
            .WithColumn("is_encrypted").AsBoolean().Nullable()
            .WithColumn("signature_id").AsString(32).Nullable()
            .WithColumn("signature_timestamp").AsDateTime().Nullable()
            .WithColumn("component_class").AsString(32).Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("aoi")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("name").Ascending()
            .WithOptions().Unique();

        Create.Index()
            .OnTable("aoi")
            .OnColumn("record_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();
    }
}