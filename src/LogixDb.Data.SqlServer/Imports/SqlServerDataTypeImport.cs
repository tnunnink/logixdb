using L5Sharp.Core;
using LogixDb.Data.Maps;

namespace LogixDb.Data.SqlServer.Imports;

/// <summary>
/// Represents a class for importing data type data into a SqlServer database.
/// </summary>
/// <remarks>
/// This class provides functionality to process and import data types into a SqlServer database
/// by using a specific set of preconfigured SQL commands and mappings. It works in
/// conjunction with a parent transaction to ensure atomic operations are performed safely.
/// </remarks>
internal class SqlServerDataTypeImport() : SqlServerElementImport<DataType>(new DataTypeMap())
{
    /// <inheritdoc />
    protected override IEnumerable<DataType> GetRecords(L5X content)
    {
        return content.Query<DataType>().ToList();
    }
}
