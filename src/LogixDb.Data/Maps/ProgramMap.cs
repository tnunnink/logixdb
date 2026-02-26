using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "program" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Program"/> class.
/// </summary>
public class ProgramMap : TableMap<Program>
{
    /// <inheritdoc />
    public override string TableName => "program";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Program>> Columns =>
    [
        ColumnMap<Program>.For(p => p.Name, "program_name"),
        ColumnMap<Program>.For(p => p.Type.Name, "program_type"),
        ColumnMap<Program>.For(p => p.Description, "description"),
        ColumnMap<Program>.For(p => p.MainRoutineName, "main_routine"),
        ColumnMap<Program>.For(p => p.FaultRoutineName, "fault_routine"),
        ColumnMap<Program>.For(p => p.Disabled, "is_disabled"),
        ColumnMap<Program>.For(p => p.UseAsFolder, "is_folder"),
        ColumnMap<Program>.For(p => p.TestEdits, "has_test_edits"),
        ColumnMap<Program>.For(p => p.Parent?.Name, "parent_name"),
        ColumnMap<Program>.For(p => p.Task?.Name, "task_name")
    ];
}