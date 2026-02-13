using L5Sharp.Core;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using Microsoft.Data.SqlClient;
using Task = System.Threading.Tasks.Task;

namespace LogixDb.SqlServer.Imports;

/// <summary>
/// Represents an abstract base class to handle the import of <typeparamref name="TElement"/> elements into a SQL Server database.
/// The process is driven by mapping the elements to database tables and using bulk copy operations.
/// </summary>
/// <typeparam name="TElement">
/// The type of element to be imported, which must implement the <see cref="ILogixElement"/> interface.
/// </typeparam>
internal abstract class SqlServerElementImport<TElement>(TableMap<TElement> map) : ILogixDatabaseImport
    where TElement : class, ILogixElement
{
    /// <summary>
    /// Executes the import process, transferring records from the provided snapshot to the SQL Server database using bulk copy.
    /// </summary>
    /// <param name="snapshot">The snapshot containing the source data to be imported.</param>
    /// <param name="session">The database session providing access to the connection and transaction.</param>
    /// <param name="token">The cancellation token used to observe cancellation requests during the import process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="NotImplementedException">Thrown if a required method or implementation is missing.</exception>
    public async Task Process(Snapshot snapshot, ILogixDatabaseSession session, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var connection = session.GetConnection<SqlConnection>();
        var transaction = session.GetTransaction<SqlTransaction>();

        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction);
        bulkCopy.DestinationTableName = $"dbo.{map.TableName}";
        var records = GetRecords(snapshot.GetSource());
        var table = map.ToDataTable(records, snapshot.SnapshotId);
        await bulkCopy.WriteToServerAsync(table, token);
    }

    /// <summary>
    /// Retrieves a collection of records of type <typeparamref name="TElement"/> from the specified content.
    /// </summary>
    /// <param name="content">The source content from which records are retrieved.</param>
    /// <returns>A collection of records of type <typeparamref name="TElement"/>.</returns>
    protected abstract IEnumerable<TElement> GetRecords(L5X content);
}