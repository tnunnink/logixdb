namespace LogixDb.Data.Abstractions;

/// <summary>
/// Defines a contract for importing specific data types from a snapshot into a database.
/// Implementations of this interface are responsible for processing and persisting
/// individual data components (such as tags, snapshots, or other entities) within
/// the context of a database session.
/// </summary>
public interface ILogixDbImport
{
    /// <summary>
    /// Processes and imports data from the provided snapshot into the database
    /// using the specified session context.
    /// </summary>
    /// <param name="snapshot">The snapshot containing the source data to be imported into the database.</param>
    /// <param name="session">The database session providing access to the connection and transaction for executing the import operations.</param>
    /// <param name="token">A cancellation token that can be used to cancel the import operation.</param>
    /// <returns>A task representing the asynchronous import operation.</returns>
    Task Process(Snapshot snapshot, ILogixDbSession session, CancellationToken token = default);
}