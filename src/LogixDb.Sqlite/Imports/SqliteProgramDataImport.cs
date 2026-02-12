using L5Sharp.Core;
using LogixDb.Core.Maps;

namespace LogixDb.Sqlite.Imports;

/// <summary>
/// Represents a class for importing program data into a SQLite database.
/// </summary>
/// <remarks>
/// This class provides functionality to process and import programs into a SQLite database
/// by using a specific set of preconfigured SQL commands and mappings. It works in
/// conjunction with a parent transaction to ensure atomic operations are performed safely.
/// </remarks>
internal class SqliteProgramImport() : SqliteElementImport<Program>(new ProgramMap())
{
    /// <inheritdoc />
    protected override IEnumerable<Program> GetRecords(L5X content)
    {
        return content.Query<Program>().ToList();
    }
}
