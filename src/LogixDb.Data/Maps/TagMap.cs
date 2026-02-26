using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "tag" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Tag"/> class.
/// </summary>
public class TagMap : TableMap<Tag>
{
    /// <inheritdoc />
    public override string TableName => "tag";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Tag>> Columns =>
    [
        ColumnMap<Tag>.For(t => t.Scope.Container, "container_name"),
        ColumnMap<Tag>.For(t => t.TagName.LocalPath, "tag_name"),
        ColumnMap<Tag>.For(t => t.TagName.Base, "base_name"),
        ColumnMap<Tag>.For(t => t.Parent?.TagName.LocalPath, "parent_name"),
        ColumnMap<Tag>.For(t => t.TagName.Element, "member_name"),
        ColumnMap<Tag>.For(t => t.Value.IsAtomic() ? t.Value.ToString() : null, "tag_value"),
        ColumnMap<Tag>.For(t => t.Dimensions.IsEmpty ? t.DataType : $"{t.DataType}{t.Dimensions.ToIndex()}", "data_type"),
        ColumnMap<Tag>.For(t => t.Description, "description"),
        ColumnMap<Tag>.For(t => t.ExternalAccess?.Name, "external_access"),
        ColumnMap<Tag>.For(t => t.Constant, "constant")
    ];
}