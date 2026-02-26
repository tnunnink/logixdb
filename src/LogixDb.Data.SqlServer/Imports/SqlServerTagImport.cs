using L5Sharp.Core;
using LogixDb.Data.Maps;

namespace LogixDb.Data.SqlServer.Imports;

/// <summary>
/// Represents a specialized implementation of <see cref="SqlServerElementImport{TElement}"/>
/// for importing <see cref="Tag"/> elements from an L5X file into a SQL Server database.
/// </summary>
/// <remarks>
/// This class uses the <see cref="TagMap"/> to define the database mapping for <see cref="Tag"/> elements
/// and processes the import by extracting tag data and its associated members from the L5X content.
/// </remarks>
/// <seealso cref="SqlServerElementImport{TElement}"/>
internal class SqlServerTagImport() : SqlServerElementImport<Tag>(new TagMap())
{
    /// <inheritdoc />
    protected override IEnumerable<Tag> GetRecords(L5X content)
    {
        return content.Query<Tag>().SelectMany(t => t.Members()).ToList();
    }
}