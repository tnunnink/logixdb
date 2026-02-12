using L5Sharp.Core;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;

namespace LogixDb.Core.Maps;

/// <summary>
/// Represents a mapping configuration for the "tag" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Tag"/> class.
/// </summary>
public class TagMap : TableMap<Tag>
{
    /// <inheritdoc />
    protected override string TableName => "tag";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Tag>> Columns =>
    [
        ColumnMap<Tag>.For(t => t.Base.TagName.LocalPath, "base_name"),
        ColumnMap<Tag>.For(t => t.TagName.LocalPath, "tag_name"),
        ColumnMap<Tag>.For(t => t.Scope.Level.Name, "scope_type"),
        ColumnMap<Tag>.For(t => t.Scope.Container, "container_name"),
        ColumnMap<Tag>.For(t => (byte)t.TagName.Depth, "tag_depth"),
        ColumnMap<Tag>.For(t => t.DataType, "data_type"),
        ColumnMap<Tag>.For(t => t.Value.IsAtomic() ? t.Value.ToString() : null, "value"),
        ColumnMap<Tag>.For(t => t.Description, "description"),
        ColumnMap<Tag>.For(t => t.Dimensions.IsEmpty ? "0" : t.Dimensions.ToIndex(), "dimensions"),
        ColumnMap<Tag>.For(t => t.ExternalAccess?.Name, "external_access"),
        ColumnMap<Tag>.For(t => t.OpcUAAccess?.Name, "opcua_access"),
        ColumnMap<Tag>.For(t => t.Radix != Radix.Null ? t.Radix.Name : null, "radix"),
        ColumnMap<Tag>.For(t => t.Constant, "constant"),
        ColumnMap<Tag>.For(t => t.TagType?.Name, "tag_type"),
        ColumnMap<Tag>.For(t => t.Usage?.Name, "tag_usage"),
        ColumnMap<Tag>.For(t => t.AliasFor?.LocalPath, "alias"),
        ColumnMap<Tag>.For(t => t.Class?.Name, "component_class"),
        ColumnMap<Tag>.For(t => t.Value.Serialize().ToString().Hash(), "value_hash")
    ];
}