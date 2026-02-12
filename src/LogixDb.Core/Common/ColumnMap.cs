using L5Sharp.Core;

namespace LogixDb.Core.Common;

/// <summary>
/// Represents a mapping configuration between a Logix element property and a database column.
/// This record defines how to extract data from an element of type <typeparamref name="TElement"/>
/// and map it to a specific database column with the appropriate type and constraints.
/// </summary>
/// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
public sealed record ColumnMap<TElement> where TElement : ILogixElement
{
    /// <summary>
    /// Gets or sets the name of the database column to which a property of <typeparamref name="TElement"/> is mapped.
    /// This represents the column's identifier in the database schema and serves as a key for data mapping.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the database column type to which a property of <typeparamref name="TElement"/> is mapped.
    /// This defines the data type representation for the column in the database schema, used for type-specific operations.
    /// </summary>
    public required ColumnType Type { get; init; }

    /// <summary>
    /// Gets the function used to extract the column value from an instance of <typeparamref name="TElement"/>.
    /// This function is invoked during data import operations to retrieve the value to be stored in the database.
    /// </summary>
    public required Func<TElement, object?> Getter { get; init; }

    /// <summary>
    /// Creates a new column map for a string-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the string value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the string property and column name.</returns>
    public static ColumnMap<TElement> For(Func<TElement, string?> getter, string name)
    {
        return new ColumnMap<TElement>
        {
            Name = name,
            Type = ColumnType.Text,
            Getter = getter
        };
    }

    /// <summary>
    /// Creates a new column map for a boolean-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the boolean value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the boolean property and column name.</returns>
    public static ColumnMap<TElement> For(Func<TElement, bool> getter, string name)
    {
        return new ColumnMap<TElement>
        {
            Name = name,
            Type = ColumnType.Boolean,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a nullable boolean-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the nullable boolean value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the nullable boolean property and column name.</returns>
    public static ColumnMap<TElement> For(Func<TElement, bool?> getter, string name)
    {
        return new ColumnMap<TElement>
        {
            Name = name,
            Type = ColumnType.Boolean,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a byte-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the byte value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the byte property and column name.</returns>
    public static ColumnMap<TElement> For(Func<TElement, byte> getter, string name)
    {
        return new ColumnMap<TElement>
        {
            Name = name,
            Type = ColumnType.Byte,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a Logix element property with a 16-bit integer (short) data type mapped to the specified database column name.
    /// </summary>
    /// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the 16-bit integer value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the 16-bit integer property and column name.</returns>
    public static ColumnMap<TElement> For(Func<TElement, short> getter, string name)
    {
        return new ColumnMap<TElement>
        {
            Name = name,
            Type = ColumnType.Int16,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for an integer-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the integer value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the integer property and column name.</returns>
    public static ColumnMap<TElement> For(Func<TElement, int> getter, string name)
    {
        return new ColumnMap<TElement>
        {
            Name = name,
            Type = ColumnType.Int32,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a float-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the float value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the float property and column name.</returns>
    public static ColumnMap<TElement> For(Func<TElement, float> getter, string name)
    {
        return new ColumnMap<TElement>
        {
            Name = name,
            Type = ColumnType.Float,
            Getter = e => getter(e)
        };
    }

    /// <summary>
    /// Creates a new column map for a DateTime-based property of a Logix element with a specified database column name.
    /// </summary>
    /// <typeparam name="TElement">The type of Logix element being mapped, which must implement <see cref="ILogixElement"/>.</typeparam>
    /// <param name="getter">A function that retrieves the DateTime value from the Logix element to be mapped to the database column.</param>
    /// <param name="name">The name of the database column to map the property to.</param>
    /// <returns>A new instance of <see cref="ColumnMap{TElement}"/> configured for the DateTime property and column name.</returns>
    public static ColumnMap<TElement> For(Func<TElement, DateTime> getter, string name)
    {
        return new ColumnMap<TElement>
        {
            Name = name,
            Type = ColumnType.DateTime,
            Getter = e => getter(e)
        };
    }
}