using System.Globalization;
using System.Text;
using L5Sharp.Core;
using LogixDb.Data.Abstractions;
using Microsoft.Data.Sqlite;
using Task = System.Threading.Tasks.Task;

namespace LogixDb.Data.Sqlite.Imports;

/// <summary>
/// Provides an abstract base class for importing database elements into an SQLite database.
/// This class maps elements of type <typeparamref name="TElement"/> to SQLite database tables
/// using the provided table mapping and performs optimized bulk insertion of data.
/// </summary>
/// <typeparam name="TElement">
/// The type of element being imported. This type must implement the <see cref="ILogixElement"/> interface.
/// </typeparam>
/// <remarks>
/// This class ensures efficient database operations by using compiled SQLite commands,
/// dynamic binding of parameters, and transaction management. Subclasses must implement
/// the abstract <see cref="GetRecords"/> method to define how to extract the records to
/// be imported from the source data.
/// </remarks>
public abstract class SqliteElementImport<TElement>(TableMap<TElement> map) : ILogixDbImport
    where TElement : class, ILogixElement
{
    /// <summary>
    /// Processes a snapshot and inserts data into the associated SQLite database session.
    /// </summary>
    /// <param name="snapshot">The snapshot containing the source data to be processed.</param>
    /// <param name="session">The active database session used for SQLite operations.</param>
    /// <param name="token">An optional cancellation token to observe during the operation.</param>
    /// <returns>A task that represents the asynchronous processing operation.</returns>
    public async Task Process(Snapshot snapshot, ILogixDbSession session, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var connection = session.GetConnection<SqliteConnection>();
        var transaction = session.GetTransaction<SqliteTransaction>();

        // Build a compiled command with configured parameters to optimize insert performance for SQLite.
        await using var command = new SqliteCommand(map.BuildInsertStatement(), connection, transaction);

        // We can configure snapshot id once for the entire process. 
        command.Parameters.Add(new SqliteParameter("@snapshot_id", snapshot.SnapshotId));

        // Use the map columns to prepare the parameter config dynamically.
        // Manually add the content hash parameter since it is not part of the column map and computed from the column values.
        var columns = map.Columns.ToList();
        columns.ForEach(c => command.Parameters.Add($"@{c.Name}", c.Type.ToSqliteType()));
        command.Parameters.Add("@record_hash", SqliteType.Blob);
        command.Prepare();

        // Precompile an ordered array of binders that map parameters to their getter function.
        // Do this once before iteration to avoid costly lookups and to make mapping explicit (by name not array index).
        var binders = columns
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .Select(c => (Param: command.Parameters[$"@{c.Name}"], c.Getter))
            .ToArray();

        // Get all source records that we would like to insert for this type.
        var records = GetRecords(snapshot.GetSource());
        var hashBuilder = new StringBuilder();

        foreach (var record in records)
        {
            hashBuilder.Clear();

            // For each column, bind the value, serialize the parameter and append to the builder.
            foreach (var binder in binders)
            {
                binder.Param.Value = binder.Getter(record) ?? DBNull.Value;
                var field = new KeyValuePair<string, object?>(binder.Param.ParameterName, binder.Param.Value);
                hashBuilder.Append(field.SerializeField());
            }

            // Update the record hash using the updated parameter bindings.
            // The record hash should always be the last parameter on the command.
            command.Parameters[^1].Value = hashBuilder.ToString().Hash();

            await command.ExecuteNonQueryAsync(token);
        }
    }

    /// <summary>
    /// Retrieves a collection of records of type <typeparamref name="TElement"/> from the specified content.
    /// </summary>
    /// <param name="content">The source content from which records are retrieved.</param>
    /// <returns>A collection of records of type <typeparamref name="TElement"/>.</returns>
    protected abstract IEnumerable<TElement> GetRecords(L5X content);
}