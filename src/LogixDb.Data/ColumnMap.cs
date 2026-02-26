using L5Sharp.Core;

namespace LogixDb.Data;

/// <summary>
/// Represents a mapping configuration between a Logix element property and a database column.
/// This record defines how to extract data from an element of type <typeparamref name="T"/>
/// and map it to a specific database column with the appropriate type and constraints.
/// </summary>
/// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
public sealed record ColumnMap<T> where T : class
{
    /// <summary>
    /// Gets or sets the name of the database column to which a property of <typeparamref name="T"/> is mapped.
    /// This represents the column's identifier in the database schema and serves as a key for data mapping.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the database column type to which a property of <typeparamref name="T"/> is mapped.
    /// This defines the data type representation for the column in the database schema, used for type-specific operations.
    /// </summary>
    public required ColumnType Type { get; init; }

    /// <summary>
    /// Gets the function used to extract the column value from an instance of <typeparamref name="T"/>.
    /// This function is invoked during data import operations to retrieve the value to be stored in the database.
    /// </summary>
    public required Func<T, object?> Getter { get; init; }

    /// <summary>
    /// Creates a new column map for a string-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the string value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the string property and column name.</returns>
    public static ColumnMap<T> For(Func<T, string?> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Text,
            Getter = getter
        };
    }

    /// <summary>
    /// Creates a new column map for a boolean-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the boolean value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the boolean property and column name.</returns>
    public static ColumnMap<T> For(Func<T, bool> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Boolean,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a nullable boolean-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the nullable boolean value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the nullable boolean property and column name.</returns>
    public static ColumnMap<T> For(Func<T, bool?> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Boolean,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a byte-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the byte value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the byte property and column name.</returns>
    public static ColumnMap<T> For(Func<T, byte> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Byte,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a nullable byte-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the nullable byte value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the nullable byte property and column name.</returns>
    public static ColumnMap<T> For(Func<T, byte?> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Byte,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a Logix element property with a 16-bit integer (short) data type mapped to the specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the 16-bit integer value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the 16-bit integer property and column name.</returns>
    public static ColumnMap<T> For(Func<T, short> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Int16,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for an integer-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the integer value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the integer property and column name.</returns>
    public static ColumnMap<T> For(Func<T, int> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Int32,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a float-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the float value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the float property and column name.</returns>
    public static ColumnMap<T> For(Func<T, float> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Float,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a DateTime-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the DateTime value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the DateTime property and column name.</returns>
    public static ColumnMap<T> For(Func<T, DateTime> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.DateTime,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a binary data property of a Logix element with the specified database column name.
    /// </summary>
    /// <typeparam name="T">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the binary data (as a byte array) from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{T}"/> configured for the binary data property and column name.</returns>
    public static ColumnMap<T> For(Func<T, byte[]> getter, string name)
    {
        return new ColumnMap<T>
        {
            Name = name,
            Type = ColumnType.Blob,
            Getter = getter
        };
    }
}