using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "rung" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Rung"/> class.
/// </summary>
public class RungMap : TableMap<Rung>
{
    /// <inheritdoc />
    public override string TableName => "rung";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Rung>> Columns =>
    [
        ColumnMap<Rung>.For(r => r.Scope.Container, "container_name"),
        ColumnMap<Rung>.For(r => r.Routine?.Name, "routine_name"),
        ColumnMap<Rung>.For(r => r.Number, "rung_number"),
        ColumnMap<Rung>.For(r => r.Comment, "comment"),
        ColumnMap<Rung>.For(r => r.Text, "code"),
        ColumnMap<Rung>.For(r => r.Text.Hash(), "code_hash")
    ];
}
