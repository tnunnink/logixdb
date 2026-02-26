using System.Data;

namespace LogixDb.Data.Abstractions;

/// <summary>
/// Represents the main interface for interacting with a LogixDb database instance.
/// Provides methods for database lifecycle management, migrations, and snapshot operations.
/// </summary>
public interface ILogixDb
{
    /// <summary>
    /// Applies pending database migrations to update the schema to the latest version.
    /// </summary>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Migrate(CancellationToken token = default);

    /// <summary>
    /// Applies pending database migrations up to the specified schema version.
    /// </summary>
    /// <param name="version">The target schema version to migrate to.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Migrate(long version, CancellationToken token = default);

    /// <summary>
    /// Drops or deletes the database, removing all tables and data.
    /// </summary>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Drop(CancellationToken token = default);

    /// <summary>
    /// Purges all data from the database while preserving the schema structure.
    /// </summary>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Purge(CancellationToken token = default);

    /// <summary>
    /// Establishes a connection to the database and returns an active database connection instance.
    /// </summary>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the opened database connection.</returns>
    Task<IDbConnection> Connect(CancellationToken token = default);

    /// <summary>
    /// Lists all snapshots in the database, optionally filtered by target key.
    /// </summary>
    /// <param name="targetKey">Optional target key to filter snapshots (format: targettype://targetname).</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a collection of snapshots.</returns>
    Task<IEnumerable<Snapshot>> ListSnapshots(string? targetKey = null, CancellationToken token = default);

    /// <summary>
    /// Retrieves the latest snapshot associated with the specified target key.
    /// </summary>
    /// <param name="targetKey">The key used to identify the target whose latest snapshot will be retrieved.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the latest snapshot for the specified target key.</returns>
    Task<Snapshot> GetSnapshotLatest(string targetKey, CancellationToken token = default);

    /// <summary>
    /// Retrieves a snapshot from the database based on the specified snapshot ID.
    /// </summary>
    /// <param name="snapshotId">The unique identifier of the snapshot to retrieve.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the specified snapshot.</returns>
    Task<Snapshot> GetSnapshotById(int snapshotId, CancellationToken token = default);

    /// <summary>
    /// Adds a snapshot to the database with the specified action.
    /// </summary>
    /// <param name="snapshot">The snapshot to be added to the database.</param>
    /// <param name="action">The action to perform when adding the snapshot (e.g., append, replace the latest, or replace all).</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddSnapshot(Snapshot snapshot, SnapshotAction action = SnapshotAction.Append,
        CancellationToken token = default);

    /// <summary>
    /// Deletes all snapshots matching the specified target key.
    /// </summary>
    /// <param name="targetKey">The target key identifying the snapshot(s) to delete (format: targettype://targetname).</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSnapshotsFor(string targetKey, CancellationToken token = default);

    /// <summary>
    /// Deletes snapshots created before the specified cutoff date. Optionally filters snapshots by a target key.
    /// </summary>
    /// <param name="importDate">The date before which snapshots will be deleted.</param>
    /// <param name="targetKey">An optional key to filter snapshots for a specific target. If null, all matching snapshots will be affected.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSnapshotsBefore(DateTime importDate, string? targetKey = null, CancellationToken token = default);

    /// <summary>
    /// Deletes the latest snapshot associated with the specified target key.
    /// </summary>
    /// <param name="targetKey">The key identifying the target whose latest snapshot should be deleted.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSnapshotLatest(string targetKey, CancellationToken token = default);

    /// <summary>
    /// Deletes a specific snapshot by its unique identifier.
    /// </summary>
    /// <param name="snapshotId">The unique identifier of the snapshot to delete.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSnapshot(int snapshotId, CancellationToken token = default);
}