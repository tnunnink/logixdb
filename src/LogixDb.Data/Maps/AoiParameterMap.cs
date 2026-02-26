using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "aoi_parameter" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Parameter"/> class.
/// </summary>
public class AoiParameterMap : TableMap<Parameter>
{
    /// <inheritdoc />
    public override string TableName => "aoi_parameter";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Parameter>> Columns =>
    [
        ColumnMap<Parameter>.For(p => p.Parent?.Name, "aoi_name"),
        ColumnMap<Parameter>.For(p => p.Name, "parameter_name"),
        ColumnMap<Parameter>.For(p => p.Dimension > 0 ? $"{p.DataType}{p.Dimension.ToIndex()}" : p.DataType, "data_type"),
        ColumnMap<Parameter>.For(p => p.Default?.IsAtomic() is true ? p.Default?.ToString() : null, "default_value"),
        ColumnMap<Parameter>.For(p => p.Description, "description"),
        ColumnMap<Parameter>.For(p => p.ExternalAccess?.Name, "external_access"),
        ColumnMap<Parameter>.For(p => p.Usage.Name, "tag_usage"),
        ColumnMap<Parameter>.For(p => p.TagType?.Name, "tag_type"),
        ColumnMap<Parameter>.For(p => p.AliasFor?.LocalPath, "tag_alias"),
        ColumnMap<Parameter>.For(p => p.Visible, "visible"),
        ColumnMap<Parameter>.For(p => p.Required, "required"),
        ColumnMap<Parameter>.For(p => p.Constant, "constant"),
    ];
}
