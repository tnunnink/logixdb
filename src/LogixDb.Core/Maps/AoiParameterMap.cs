using L5Sharp.Core;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;

namespace LogixDb.Core.Maps;

/// <summary>
/// Represents a mapping configuration for the "aoi_parameter" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Parameter"/> class.
/// </summary>
public class AoiParameterMap : TableMap<Parameter>
{
    /// <inheritdoc />
    protected override string TableName => "aoi_parameter";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Parameter>> Columns =>
    [
        ColumnMap<Parameter>.For(p => p.Name, "name"),
        ColumnMap<Parameter>.For(p => p.Dimension > 0 ? $"{p.DataType}{p.Dimension.ToIndex()}" : p.DataType, "data_type"),
        ColumnMap<Parameter>.For(p => p.Default?.IsAtomic() is true ? p.Default?.ToString() : null, "value"),
        ColumnMap<Parameter>.For(p => p.Description, "description"),
        ColumnMap<Parameter>.For(p => p.ExternalAccess?.Name, "external_access"),
        ColumnMap<Parameter>.For(p => p.Usage.Name, "tag_usage"),
        ColumnMap<Parameter>.For(p => p.TagType?.Name, "tag_type"),
        ColumnMap<Parameter>.For(p => p.AliasFor?.LocalPath, "alias"),
        ColumnMap<Parameter>.For(p => p.Visible, "visible"),
        ColumnMap<Parameter>.For(p => p.Required, "required"),
        ColumnMap<Parameter>.For(p => p.Constant, "constant"),
        ColumnMap<Parameter>.For(p => LogixType.IsAtomic(p.DataType), "is_atomic"),
        ColumnMap<Parameter>.For(p => p.Dimension.Length > 0, "is_array")
    ];
}
