using L5Sharp.Core;
using LogixDb.Data.Maps;

namespace LogixDb.Data.SqlServer.Imports;

/// <summary>
/// Provides functionality to import Add-On Instruction (AOI) elements into a SQL Server database.
/// This class is responsible for retrieving AOI records from an L5X content source and mapping them to a database structure for import.
/// Derived from <see cref="SqlServerElementImport{TElement}"/>, where TElement is <see cref="AddOnInstruction"/>.
/// </summary>
internal class SqlServerAoiImport() : SqlServerElementImport<AddOnInstruction>(new AoiMap())
{
    /// <inheritdoc />
    protected override IEnumerable<AddOnInstruction> GetRecords(L5X content)
    {
        return content.Query<AddOnInstruction>().ToList();
    }
}
