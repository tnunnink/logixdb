using L5Sharp.Core;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;

namespace LogixDb.Core.Maps;

/// <summary>
/// Represents a mapping configuration for the "data_type" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="DataType"/> class.
/// </summary>
public class DataTypeMap : TableMap<DataType>
{
    /// <inheritdoc />
    protected override string TableName => "data_type";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<DataType>> Columns =>
    [
        ColumnMap<DataType>.For(t => t.Name, "name"),
        ColumnMap<DataType>.For(t => t.Class?.Name, "type_class"),
        ColumnMap<DataType>.For(t => t.Family?.Name, "type_family"),
        ColumnMap<DataType>.For(t => t.Description, "description")
    ];
}