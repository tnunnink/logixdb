using System.Data;
using Dapper;
using FluentMigrator.Runner;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using LogixDb.Core.Exceptions;
using LogixDb.Migrations;
using LogixDb.Sqlite.Imports;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Sqlite;

/// <summary>
/// Represents an SQLite-backed implementation of the <see cref="ILogixDb"/> interface.
/// This class provides methods to manage database migrations, snapshots, and data import/export processes
/// within an SQLite database.
/// </summary>
public sealed class SqliteDb : ILogixDb
{
    /// <summary>
    /// Represents the connection information required for interacting with an SQLite database.
    /// This variable contains details such as the database file path, credentials, and other
    /// configuration parameters encapsulated in a <see cref="SqlConnectionInfo"/> instance.
    /// It serves as the primary connection descriptor for database operations.
    /// </summary>
    private readonly SqlConnectionInfo _connection;

    /// <summary>
    /// Represents a service provider for managing database migrations in the SQLite implementation.
    /// This provider enables the execution of migration tasks such as applying or rolling back schema updates
    /// to ensure that the database schema is current and consistent.
    /// </summary>
    private readonly ServiceProvider _migrationProvider;

    /// <summary>
    /// Represents the collection of imports used in the Sqlite database implementation.
    /// These imports are responsible for mapping data elements between SQLite and
    /// the application domain, enabling seamless data operations.
    /// </summary>
    private readonly List<ILogixDbImport> _imports =
    [
        new SqliteSnapshotImport(),
        new SqliteControllerImport(),
        new SqliteDataTypeImport(),
        new SqliteDataTypeMemberImport(),
        new SqliteAoiImport(),
        new SqliteAoiParameterImport(),
        new SqliteModuleImport(),
        new SqliteTaskImport(),
        new SqliteProgramImport(),
        new SqliteRoutineImport(),
        new SqliteRungImport(),
        new SqliteTagImport(),
    ];

    /// <summary>
    /// Represents an SQLite-backed implementation of the <see cref="ILogixDb"/> interface.
    /// This class provides methods to manage database migrations, snapshots, and data import/export processes
    /// within an SQLite database.
    /// </summary>
    public SqliteDb(SqlConnectionInfo connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _migrationProvider = BuildMigrationProvider(_connection.ToConnectionString());
    }

    /// <inheritdoc />
    public async Task Migrate(CancellationToken token = default)
    {
        var runner = _migrationProvider.GetRequiredService<IMigrationRunner>();
        await Task.Run(() => runner.MigrateUp(), token);
        // After migration of the database enable performance enhancing PRAGMA settings.
        await ConfigurePersistentPerformancePragmas(token);
    }

    /// <inheritdoc />
    public async Task Migrate(long version, CancellationToken token = default)
    {
        var runner = _migrationProvider.GetRequiredService<IMigrationRunner>();
        await Task.Run(() => runner.MigrateUp(version), token);
        // After migration of the database enable performance enhancing PRAGMA settings.
        await ConfigurePersistentPerformancePragmas(token);
    }

    /// <inheritdoc />
    public Task Drop(CancellationToken token = default)
    {
        if (File.Exists(_connection.DataSource))
            File.Delete(_connection.DataSource);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task Purge(CancellationToken token = default)
    {
        EnsureMigrated();
        await using var connection = await OpenConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        try
        {
            await connection.ExecuteAsync("DELETE FROM target WHERE target_id > 0", transaction);
            await transaction.CommitAsync(token);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IDbConnection> Connect(CancellationToken token = default)
    {
        return await OpenConnectionAsync(token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Snapshot>> ListSnapshots(string? targetKey = null, CancellationToken token = default)
    {
        EnsureMigrated();
        await using var connection = await OpenConnectionAsync(token);
        var key = new { target_key = targetKey };
        return await connection.QueryAsync<Snapshot>(SqliteQuery.ListSnapshots, key);
    }

    /// <inheritdoc />
    public async Task AddSnapshot(Snapshot snapshot, CancellationToken token = default)
    {
        EnsureMigrated();
        await using var session = await SqliteDbSession.StartAsync(this, token);

        try
        {
            foreach (var import in _imports)
                await import.Process(snapshot, session, token);

            await session.GetTransaction<SqliteTransaction>().CommitAsync(token);
        }
        catch (Exception)
        {
            await session.GetTransaction<SqliteTransaction>().RollbackAsync(token);
            throw;
        }
    }

    public Task UpsertSnapshot(Snapshot snapshot, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<Snapshot> GetSnapshotById(int snapshotId, CancellationToken token = default)
    {
        EnsureMigrated();
        await using var connection = await OpenConnectionAsync(token);
        var key = new { snapshot_id = snapshotId };
        return await connection.QuerySingleAsync<Snapshot>(SqliteQuery.GetSnapshotById, key);
    }

    /// <inheritdoc />
    public async Task DeleteSnapshots(string targetKey, CancellationToken token = default)
    {
        EnsureMigrated();
        const string sql = """
                           DELETE FROM snapshot 
                           WHERE target_id = (SELECT target_id FROM target WHERE target_key = @target_key);
                           """;
        await using var connection = await OpenConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        try
        {
            await connection.ExecuteAsync(sql, new { target_key = targetKey }, transaction);
            await transaction.CommitAsync(token);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteSnapshot(int snapshotId, CancellationToken token = default)
    {
        EnsureMigrated();
        const string sql = "DELETE FROM snapshot WHERE snapshot_id = @snapshot_id;";
        await using var connection = await OpenConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        try
        {
            await connection.ExecuteAsync(sql, new { snapshot_id = snapshotId }, transaction);
            await transaction.CommitAsync(token);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }

    /// <summary>
    /// Establishes and opens an asynchronous connection to the SQLite database using the configured connection string.
    /// </summary>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an open database connection.</returns>
    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken token)
    {
        var connection = new SqliteConnection(_connection.ToConnectionString());
        await connection.OpenAsync(token);
        return connection;
    }

    /// <summary>
    /// Ensures that all pending migrations have been applied to the database.
    /// </summary>
    /// <exception cref="MigrationRequiredException">
    /// Thrown when there are unapplied migrations that need to be applied to bring
    /// the database to the required state.
    /// </exception>
    private void EnsureMigrated()
    {
        var runner = _migrationProvider.GetRequiredService<IMigrationRunner>();

        if (runner.HasMigrationsToApplyUp())
        {
            throw new MigrationRequiredException();
        }
    }

    /// <summary>
    /// Creates a migration provider for managing database migrations using FluentMigrator.
    /// Configures the service collection with the necessary settings for SQLite integration,
    /// including connection string, version table metadata, and migration scanning.
    /// </summary>
    /// <param name="connectionString">The connection string for the SQLite database.</param>
    /// <returns>A configured <see cref="ServiceProvider"/> instance to execute migrations.</returns>
    private static ServiceProvider BuildMigrationProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .WithVersionTable(new MigrationTableMetaData())
                .ScanIn(
                    typeof(MigrationTableMetaData).Assembly,
                    typeof(SqliteDb).Assembly)
                .For.Migrations()
            );

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Configures SQLite performance-related PRAGMA settings to enhance database operations.
    /// This method is responsible for setting various SQLite pragmas, such as journal mode,
    /// auto vacuum mode, synchronization settings, and memory usage configurations, 
    /// to optimize the database performance.
    /// </summary>
    /// <remarks>
    /// The method adjusts settings including WAL (Write-Ahead Logging) for journaling,
    /// incremental vacuum, and memory configurations such as mmap size and cache size.
    /// These settings are tailored to improve performance while maintaining data integrity.
    /// </remarks>
    private async Task ConfigurePersistentPerformancePragmas(CancellationToken token)
    {
        var connection = await OpenConnectionAsync(token);

        await connection.ExecuteAsync(
            """
            PRAGMA journal_mode = WAL;
            PRAGMA auto_vacuum = FULL;
            PRAGMA temp_store = MEMORY;
            PRAGMA busy_timeout = 5000;
            PRAGMA page_size = 16384;
            """
        );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _migrationProvider.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _migrationProvider.DisposeAsync();
    }
}