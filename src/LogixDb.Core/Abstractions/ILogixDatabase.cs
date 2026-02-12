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
    /// Establishes a connection to the database if it exists, or creates a new database if it does not.
    /// This method ensures that the database is initialized with the necessary structure and configuration,
    /// enabling further operations to be performed. Database migrations are applied in cases
    /// where a new database is being created.
    /// </summary>
    /// <remarks>
    /// If the database does not exist, this method will create and migrate the database to the latest version automatically.
    /// If the database exists and is up to date, then we set internal connection parameters and return.
    /// If the database exists but migrations are required, this method will throw the <see cref="MigrationRequiredException"/>.
    /// </remarks>
    void ConnectOrCreate();

    /// <summary>
    /// Executes database schema migrations to ensure the database structure is up to date.
    /// This method should be called during application startup or deployment to apply any pending migrations.
    /// </summary>
    void Migrate();

    /// <summary>
    /// Retrieves all snapshots from the database, optionally filtered by a specific target key.
    /// </summary>
    /// <param name="targetKey">Optional target key in the format "targettype://targetname" to filter snapshots. If null, returns all snapshots.</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A collection of snapshots matching the specified criteria, or all snapshots if no target key is provided.</returns>
    Task<IEnumerable<Snapshot>> Snapshots(string? targetKey = null, CancellationToken token = default);

    /// <summary>
    /// Imports a snapshot into the database, creating a new snapshot record with associated metadata.
    /// If a target key is not provided, the snapshot's default key will be used.
    /// </summary>
    /// <param name="snapshot">The snapshot instance to import containing L5X data and metadata.</param>
    /// <param name="targetKey">Optional target key override for storing the snapshot.
    /// If null, uses the snapshot's default key from GetDefaultKey().</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The imported snapshot with the assigned SnapshotId from the database.</returns>
    Task<Snapshot> Import(Snapshot snapshot, string? targetKey = null, CancellationToken token = default);

    /// <summary>
    /// Exports the most recent snapshot for the specified target key from the database.
    /// Retrieves the snapshot with its compressed source data and metadata.
    /// </summary>
    /// <param name="targetKey">The target key in the format "targettype://targetname" identifying which snapshot to export.</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>The most recent snapshot matching the specified target key.</returns>
    Task<Snapshot> Export(string targetKey, CancellationToken token = default);

    /// <summary>
    /// Purges all snapshots from the database, removing all snapshot records and associated data.
    /// This operation permanently deletes all stored snapshot history.
    /// </summary>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous purge operation.</returns>
    Task Purge(CancellationToken token = default);

    /// <summary>
    /// Purges all snapshots associated with a specific target key from the database.
    /// This operation permanently deletes all snapshot history for the specified target.
    /// </summary>
    /// <param name="targetKey">The target key in the format "targettype://targetname" identifying which snapshots to purge.</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous purge operation.</returns>
    Task Purge(string targetKey, CancellationToken token = default);
}