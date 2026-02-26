using LogixDb.Data.Abstractions;
using Task = L5Sharp.Core.Task;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "task" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Task"/> class.
/// </summary>
public class TaskMap : TableMap<Task>
{
    /// <inheritdoc />
    public override string TableName => "task";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Task>> Columns =>
    [
        ColumnMap<Task>.For(t => t.Name, "task_name"),
        ColumnMap<Task>.For(t => t.Type.Name, "task_type"),
        ColumnMap<Task>.For(t => t.Description, "description"),
        ColumnMap<Task>.For(t => t.Priority, "priority"),
        ColumnMap<Task>.For(t => t.Rate, "rate"),
        ColumnMap<Task>.For(t => t.Watchdog, "watchdog"),
        ColumnMap<Task>.For(t => t.InhibitTask, "inhibited"),
        ColumnMap<Task>.For(t => t.DisableUpdateOutputs, "disable_outputs"),
        ColumnMap<Task>.For(t => t.EventInfo?.EventTrigger?.Name, "event_trigger"),
        ColumnMap<Task>.For(t => t.EventInfo?.EventTag?.LocalPath, "event_tag"),
        ColumnMap<Task>.For(t => t.EventInfo?.EnableTimeout, "enable_timeout")
    ];
}