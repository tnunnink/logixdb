using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260212;

[UsedImplicitly]
[Migration(202602120900, "Creates aoi_parameter table with corresponding indexes and keys")]
public class M012CreateAoiParameterTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("aoi_parameter")
            .WithPrimaryId("parameter_id")
            .WithCascadeForeignKey("aoi_id", "aoi")
            .WithColumn("name").AsString(128).NotNullable()
            .WithColumn("data_type").AsString(128).Nullable()
            .WithColumn("value").AsString(128).Nullable()
            .WithColumn("description").AsString(512).Nullable()
            .WithColumn("external_access").AsString(32).Nullable()
            .WithColumn("tag_usage").AsString(32).Nullable()
            .WithColumn("tag_type").AsString(32).Nullable()
            .WithColumn("alias").AsString(128).Nullable()
            .WithColumn("visible").AsBoolean().Nullable()
            .WithColumn("required").AsBoolean().Nullable()
            .WithColumn("constant").AsBoolean().Nullable()
            .WithColumn("is_atomic").AsBoolean().NotNullable()
            .WithColumn("is_array").AsBoolean().NotNullable()
            .WithColumn("record_hash").AsString(32).NotNullable();

        Create.Index()
            .OnTable("aoi_parameter")
            .OnColumn("aoi_id").Ascending()
            .OnColumn("name").Ascending()
            .WithOptions().Unique();
    }
}