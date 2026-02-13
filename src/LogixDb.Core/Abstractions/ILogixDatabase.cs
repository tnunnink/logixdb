using LogixDb.Core.Common;
using LogixDb.Core.Exceptions;

namespace LogixDb.Core.Abstractions;

/// <summary>
/// Defines the core contract for managing LogixDB database operations.
/// Provides methods for importing, exporting, and managing snapshots of Logix controller data,
/// along with database migration and purge operations for maintaining the snapshot repository.
/// </summary>
public interface ILogixDatabase
{
    /// <summary>
    /// Initializes or rebuilds the database. This method ensures that the database structure
    /// is created or reset as necessary, depending on the specified parameters.
    /// </summary>
    /// <param name="rebuild">
    /// A boolean flag indicating whether the database should be rebuilt. If set to true, the database
    /// will be reset to its initial state prior to rebuilding.
    /// </param>
    /// <param name="token">
    /// A cancellation token that can be used to cancel the operation before completion.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation of building or rebuilding the database.
    /// </returns>
    Task Build(bool rebuild = false, CancellationToken token = default);

    /// <summary>
    /// Applies any pending migrations to an existing database. If no migrations are pending,
    /// this method completes without making any changes. Typically used to ensure that the database
    /// schema is up to date and matches the current state defined by the migration scripts.
    /// </summary>
    void Migrate();

    /// <summary>
    /// Retrieves all snapshots from the database, optionally filtered by a specific target key.
    /// </summary>
    /// <param name="targetKey">Optional target key in the format "targettype://targetname" to filter snapshots. If null, returns all snapshots.</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A collection of snapshots matching the specified criteria, or all snapshots if no target key is provided.</returns>
    Task<IEnumerable<Snapshot>> ListSnapshots(string? targetKey = null, CancellationToken token = default);

    /// <summary>
    /// Exports the most recent snapshot for the specified target key from the database.
    /// Retrieves the snapshot with its compressed source data and metadata.
    /// </summary>
    /// <param name="targetKey">The target key in the format "targettype://targetname" identifying which snapshot to export.</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The most recent snapshot matching the specified target key.</returns>
    Task<Snapshot> GetSnapshot(string targetKey, CancellationToken token = default);

    /// <summary>
    /// Imports a snapshot into the database, creating a new snapshot record with associated metadata.
    /// If a target key is not provided, the snapshot's default key will be used.
    /// </summary>
    /// <param name="snapshot">The snapshot instance to import containing L5X data and metadata.</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The imported snapshot with the assigned SnapshotId from the database.</returns>
    Task<Snapshot> AddSnapshot(Snapshot snapshot, CancellationToken token = default);

    /// <summary>
    /// Purges all snapshots from the database, removing all snapshot records and associated data.
    /// This operation permanently deletes all stored snapshot history.
    /// </summary>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous purge operation.</returns>
    Task PurgeSnapshots(CancellationToken token = default);

    /// <summary>
    /// Purges all snapshots associated with a specific target key from the database.
    /// This operation permanently deletes all snapshot history for the specified target.
    /// </summary>
    /// <param name="targetKey">The target key in the format "targettype://targetname" identifying which snapshots to purge.</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous purge operation.</returns>
    Task DeleteSnapshot(string targetKey, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="snapshotId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    //Task DeleteSnapshot(int snapshotId, CancellationToken token = default);
}