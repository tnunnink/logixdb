using L5Sharp.Core;
using LogixDb.Data.Maps;

namespace LogixDb.Data.SqlServer.Imports;

/// <summary>
/// A class responsible for importing controller data from an L5X file into an SqlServer database.
/// Implements element import for <see cref="Controller"/> objects by extracting the single
/// controller instance from the L5X content and mapping it to the database using <see cref="ControllerMap"/>.
/// </summary>
internal class SqlServerControllerImport() : SqlServerElementImport<Controller>(new ControllerMap())
{
    /// <inheritdoc />
    protected override IEnumerable<Controller> GetRecords(L5X content)
    {
        return [content.Controller];
    }
}
