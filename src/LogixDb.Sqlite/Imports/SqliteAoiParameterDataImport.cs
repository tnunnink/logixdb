using L5Sharp.Core;
using LogixDb.Core.Maps;

namespace LogixDb.Sqlite.Imports;

/// <summary>
/// A class responsible for importing AOI (Add-On Instruction) parameter data from an L5X file into an SQLite database.
/// Implements element import for <see cref="Parameter"/> objects by extracting all parameters from all
/// Add-On Instructions in the L5X content and mapping them to the database using <see cref="AoiParameterMap"/>.
/// </summary>
internal class SqliteAoiParameterImport() : SqliteElementImport<Parameter>(new AoiParameterMap())
{
    /// <inheritdoc />
    protected override IEnumerable<Parameter> GetRecords(L5X content)
    {
        return content.Query<AddOnInstruction>().SelectMany(aoi => aoi.Parameters).ToList();
    }
}
