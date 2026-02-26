using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for LocalTag objects to the "aoi_parameter" database table.
/// This class defines how specific properties of LocalTag elements are mapped to corresponding table columns.
/// </summary>
/// <remarks>
/// This is my way of combining the local tags and parameters for an AOI into a single table in the database.
/// They are essentially all the same columns, except that the local tag doesn't have required and visible properties.
/// I'd prefer not to make separate tables and not to tread these as "tag" instances but as member definitions.
/// </remarks>
public class AoiLocalTagMap : TableMap<LocalTag>
{
    /// <inheritdoc />
    public override string TableName => "aoi_parameter";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<LocalTag>> Columns =>
    [
        ColumnMap<LocalTag>.For(p => p.Instruction?.Name, "aoi_name"),
        ColumnMap<LocalTag>.For(p => p.Name, "parameter_name"),
        ColumnMap<LocalTag>.For(p => p.Dimensions > 0 ? $"{p.DataType}{p.Dimensions.ToIndex()}" : p.DataType, "data_type"),
        ColumnMap<LocalTag>.For(p => p.Value.IsAtomic()? p.Value.ToString() : null, "default_value"),
        ColumnMap<LocalTag>.For(p => p.Description, "description"),
        ColumnMap<LocalTag>.For(p => p.ExternalAccess?.Name, "external_access"),
        ColumnMap<LocalTag>.For(p => p.Usage?.Name, "tag_usage"),
        ColumnMap<LocalTag>.For(p => p.TagType?.Name, "tag_type"),
        ColumnMap<LocalTag>.For(p => p.AliasFor?.LocalPath, "tag_alias"),
        ColumnMap<LocalTag>.For(p => p.Constant, "constant"),
    ];
}