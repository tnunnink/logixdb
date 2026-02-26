using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "data_type_member" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="DataTypeMember"/> class.
/// </summary>
public class DataTypeMemberMap : TableMap<DataTypeMember>
{
    /// <inheritdoc />
    public override string TableName => "data_type_member";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<DataTypeMember>> Columns =>
    [
        ColumnMap<DataTypeMember>.For(m => m.Parent?.Name, "type_name"),
        ColumnMap<DataTypeMember>.For(m => m.Name, "member_name"),
        ColumnMap<DataTypeMember>.For(m => m.DataType, "data_type"),
        ColumnMap<DataTypeMember>.For(m => m.Dimension, "dimension"),
        ColumnMap<DataTypeMember>.For(m => m.Radix?.Name, "radix"),
        ColumnMap<DataTypeMember>.For(m => m.ExternalAccess?.Name, "external_access"),
        ColumnMap<DataTypeMember>.For(m => m.Description, "description"),
        ColumnMap<DataTypeMember>.For(m => m.Hidden, "hidden"),
        ColumnMap<DataTypeMember>.For(m => m.Target, "target"),
        ColumnMap<DataTypeMember>.For(m => m.BitNumber is not null ? (byte)m.BitNumber : null, "bit_number")
    ];
}
