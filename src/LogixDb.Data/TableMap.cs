using System.Data;
using System.Text;

namespace LogixDb.Data;

/// <summary>
/// Defines an abstract base class for mapping Logix elements to database table structures.
/// Provides the schema definition for how a specific type of Logix element should be stored in the database,
/// including the table name and column mappings.
/// </summary>
/// <typeparam name="T">The type of Logix element this table map represents must implement ILogixElement.</typeparam>
public abstract class TableMap<T> where T : class
{
    /// <summary>
    /// Gets the name of the database table that will store the mapped Logix elements.
    /// </summary>
    public abstract string TableName { get; }

    /// <summary>
    /// Gets the collection of column mappings that define how properties of the Logix element
    /// are mapped to columns in the database table.
    /// </summary>
    public abstract IReadOnlyList<ColumnMap<T>> Columns { get; }

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

    /// <summary>
    /// Converts a collection of objects of type T into a DataTable using the column mappings
    /// defined in the implementing TableMap. The resulting DataTable includes a snapshot identifier,
    /// columns mapped from the object properties, and a computed record hash for each row.
    /// </summary>
    /// <param name="records">The collection of objects of type T to be converted into a DataTable.</param>
    /// <param name="snapshotId">An optional snapshot identifier to be added to each row. Defaults to 0 if not specified.</param>
    /// <returns>
    /// A DataTable object with the column structure defined by the TableMap, populated with data
    /// from the input records, along with a "snapshot_id" column and a computed "record_hash" column.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="records"/> parameter is null.
    /// </exception>
    /// <exception cref="Exception">
    /// May be thrown during the data processing or column mapping if unexpected errors occur.
    /// </exception>
    public DataTable GenerateTable(IEnumerable<T> records, int snapshotId = 0)
    {
        var table = new DataTable(TableName);

        table.Columns.Add(new DataColumn("snapshot_id", typeof(int)));
        table.Columns.AddRange(Columns.Select(c => new DataColumn(c.Name, c.Type.ToType())).ToArray());
        table.Columns.Add(new DataColumn("record_hash", typeof(byte[])));

        // Precompile an ordered array of column name and getter pairs for iteration. We need to iterate in deterministic
        // order to preserve the integrity of the computed hash. 
        var orderedColumns = Columns
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .ToDictionary(c => c.Name, c => c.Getter);

        var hashBuilder = new StringBuilder();
        table.BeginLoadData();

        foreach (var record in records)
        {
            hashBuilder.Clear();

            // Start a new row and set the snapshot which by default is expected to the first column.
            var row = table.NewRow();
            row[0] = snapshotId;

            // Iterate the ordered columns, get the corresponding index, set the row field to the value of the getter,
            // and compute the hash for the record. Doing this all in one pass makes this as fast as possible.
            foreach (var (name, getter) in orderedColumns)
            {
                var value = getter(record) ?? DBNull.Value;
                row[name] = value;
                var field = new KeyValuePair<string, object?>(name, value);
                hashBuilder.Append(field.SerializeField());
            }

            // record_hash is the last column of the table.
            row[table.Columns.Count - 1] = hashBuilder.ToString().Hash();

            // Add the row to the table
            table.Rows.Add(row);
        }

        return table;
    }
}