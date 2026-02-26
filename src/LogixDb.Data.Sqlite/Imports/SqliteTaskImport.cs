using L5Sharp.Core;
using LogixDb.Data.Maps;
using Task = L5Sharp.Core.Task;

namespace LogixDb.Data.Sqlite.Imports;

/// <summary>
/// Represents a class for importing task data into a SQLite database.
/// </summary>
/// <remarks>
/// This class provides functionality to process and import tasks into a SQLite database
/// by using a specific set of preconfigured SQL commands and mappings. It works in
/// conjunction with a parent transaction to ensure atomic operations are performed safely.
/// </remarks>
internal class SqliteTaskImport() : SqliteElementImport<Task>(new TaskMap())
{
    /// <inheritdoc />
    protected override IEnumerable<Task> GetRecords(L5X content)
    {
        return content.Query<Task>().ToList();
    }
}
