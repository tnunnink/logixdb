using L5Sharp.Core;
using LogixDb.Core.Maps;

namespace LogixDb.SqlServer.Imports;

/// <summary>
/// 
/// </summary>
internal class SqlServerTagImport() : SqlServerElementImport<Tag>(new TagMap())
{
    protected override IEnumerable<Tag> GetRecords(L5X content)
    {
        return content.Query<Tag>().SelectMany(t => t.Members()).ToList();
    }
}