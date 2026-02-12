using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260212;

[UsedImplicitly]
[Migration(202602121000, "Creates cross_reference table with corresponding indexes and keys")]
public class M014CreateCrossReferenceTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("cross_reference")
            .WithPrimaryId("reference_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("source_type").AsString(32).NotNullable()
            .WithColumn("source_scope").AsString(32).NotNullable()
            .WithColumn("source_container").AsString(128).Nullable()
            .WithColumn("source_routine").AsString(128).Nullable()
            .WithColumn("source_id").AsString(128).NotNullable()
            .WithColumn("source_element").AsString(512).Nullable()
            .WithColumn("target_type").AsString(32).NotNullable()
            .WithColumn("target_scope").AsString(32).NotNullable()
            .WithColumn("target_container").AsString(128).Nullable()
            .WithColumn("target_id").AsString(128).NotNullable();
        
        Create.Index()
            .OnTable("cross_reference")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("source_type").Ascending()
            .OnColumn("source_scope").Ascending()
            .OnColumn("source_container").Ascending()
            .OnColumn("source_routine").Ascending()
            .OnColumn("source_id").Ascending()
            .OnColumn("source_element").Ascending();

        Create.Index()
            .OnTable("cross_reference")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("target_type").Ascending()
            .OnColumn("target_scope").Ascending()
            .OnColumn("target_container").Ascending()
            .OnColumn("target_id").Ascending();
    }
}