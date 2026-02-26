namespace LogixDb.Data;

/// <summary>
/// Defines the supported data types for database columns.
/// This enumeration is used to specify the type of data a column can store.
/// </summary>
public enum ColumnType
{
    Boolean,
    Byte,
    Int16,
    Int32,
    Float,
    Text,
    DateTime,
    Blob
}

public static class ColumnTypeExtensions
{
    /// <summary>
    /// Converts a ColumnType enum value to its corresponding .NET Type.
    /// </summary>
    /// <param name="type">The ColumnType enum value to convert.</param>
    /// <returns>The .NET Type corresponding to the specified ColumnType.</returns>
    public static Type ToType(this ColumnType type)
    {
        return type switch
        {
            ColumnType.Boolean => typeof(bool),
            ColumnType.Byte => typeof(byte),
            ColumnType.Int16 => typeof(short),
            ColumnType.Int32 => typeof(int),
            ColumnType.Float => typeof(float),
            ColumnType.Text => typeof(string),
            ColumnType.DateTime => typeof(DateTime),
            ColumnType.Blob => typeof(byte[]),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported column type")
        };
    }
}