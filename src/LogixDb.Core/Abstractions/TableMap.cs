using L5Sharp.Core;
using LogixDb.Core.Common;

namespace LogixDb.Core.Abstractions;

/// <summary>
/// Defines an abstract base class for mapping Logix elements to database table structures.
/// Provides the schema definition for how a specific type of Logix element should be stored in the database,
/// including the table name and column mappings.
/// </summary>
/// <typeparam name="TElement">The type of Logix element this table map represents must implement ILogixElement.</typeparam>
public abstract class TableMap<TElement> where TElement : ILogixElement
{
    /// <summary>
    /// Gets the name of the database table that will store the mapped Logix elements.
    /// </summary>
    protected abstract string TableName { get; }

    /// <summary>
    /// Gets the collection of column mappings that define how properties of the Logix element
    /// are mapped to columns in the database table.
    /// </summary>
    public abstract IReadOnlyList<ColumnMap<TElement>> Columns { get; }

    /// <summary>
    /// Constructs an SQL INSERT statement for a table defined by the implementing TableMap.
    /// The statement includes named parameters corresponding to the table's columns and
    /// additional fields such as a snapshot ID and record hash.
    /// </summary>
    /// <returns>
    /// A string representing the SQL INSERT statement including column names and parameters.
    /// The resulting statement follows the format:
    /// "INSERT INTO {TableName} (snapshot_id, {column names}, record_hash)
    /// VALUES (@snapshot_id, {parameters}, @record_hash);"
    /// </returns>
    public string BuildInsertStatement()
    {
        var columns = string.Join(", ", Columns.Select(c => c.Name));
        var parameters = string.Join(", ", Columns.Select(c => $"@{c.Name}"));

        return $"""
                INSERT INTO {TableName} (snapshot_id, {columns}, record_hash)
                VALUES (@snapshot_id, {parameters}, @record_hash);
                """;
    }
}