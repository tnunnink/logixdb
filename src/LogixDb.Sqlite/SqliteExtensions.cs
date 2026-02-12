using LogixDb.Core.Common;
using Microsoft.Data.Sqlite;

namespace LogixDb.Sqlite;

/// <summary>
/// Provides extension methods for working with SQLite database types in the LogixDb context.
/// </summary>
internal static class SqliteExtensions
{
    /// <summary>
    /// Converts a <see cref="ColumnType"/> to its equivalent <see cref="SqliteType"/>.
    /// </summary>
    /// <param name="columnType">The column type to be converted.</param>
    /// <returns>The corresponding <see cref="SqliteType"/> for the specified <paramref name="columnType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the provided <paramref name="columnType"/> is not supported by the conversion.
    /// </exception>
    public static SqliteType ToSqliteType(this ColumnType columnType)
    {
        return columnType switch
        {
            ColumnType.Boolean or ColumnType.Byte or ColumnType.Int16 or ColumnType.Int32 => SqliteType.Integer,
            ColumnType.Float => SqliteType.Real,
            ColumnType.Text or ColumnType.DateTime => SqliteType.Text,
            ColumnType.Blob => SqliteType.Blob,
            _ => throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null)
        };
    }
}