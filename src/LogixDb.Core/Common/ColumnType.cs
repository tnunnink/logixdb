namespace LogixDb.Core.Common;

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