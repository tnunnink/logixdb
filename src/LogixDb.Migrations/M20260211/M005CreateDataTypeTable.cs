using FluentMigrator;
using JetBrains.Annotations;

namespace LogixDb.Migrations.M20260211;

[UsedImplicitly]
[Migration(202602111500, "Create the data type table with associated keys and indexes.")]
public class M005CreateDataTypeTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("data_type")
            .WithPrimaryId("type_id")
            .WithCascadeForeignKey("snapshot_id", "snapshot")
            .WithColumn("name").AsString(128).NotNullable()
            .WithColumn("type_class").AsString(32).Nullable()
            .WithColumn("type_family").AsString(32).Nullable()
            .WithColumn("description").AsString(int.MaxValue).Nullable()
            .WithColumn("record_hash").AsString(32).NotNullable();
        
        Create.Index()
            .OnTable("data_type")
            .OnColumn("snapshot_id").Ascending()
            .OnColumn("name").Ascending()
            .WithOptions().Unique();

        Create.Index()
            .OnTable("data_type")
            .OnColumn("record_hash").Ascending()
            .OnColumn("snapshot_id").Ascending();
    }
}