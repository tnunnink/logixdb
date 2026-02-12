using L5Sharp.Core;
using LogixDb.Core.Maps;

namespace LogixDb.Sqlite.Imports;

/// <summary>
/// Represents a class for importing Add-On Instruction (AOI) data into a SQLite database.
/// </summary>
/// <remarks>
/// This class provides functionality to process and import AOIs into a SQLite database
/// by using a specific set of preconfigured SQL commands and mappings. It works in
/// conjunction with a parent transaction to ensure atomic operations are performed safely.
/// </remarks>
internal class SqliteAoiImport() : SqliteElementImport<AddOnInstruction>(new AoiMap())
{
    /// <inheritdoc />
    protected override IEnumerable<AddOnInstruction> GetRecords(L5X content)
    {
        return content.Query<AddOnInstruction>().ToList();
    }
}
