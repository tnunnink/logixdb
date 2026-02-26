using System.Data;
using Dapper;
using FluentMigrator.Runner;
using LogixDb.Data.Abstractions;
using LogixDb.Data.Exceptions;
using LogixDb.Data.Sqlite.Imports;
using LogixDb.Migrations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Data.Sqlite;

/// <summary>
/// Represents an SQLite-backed implementation of the <see cref="ILogixDb"/> interface.
/// This class provides methods to manage database migrations, snapshots, and data import/export processes
/// within an SQLite database.
/// </summary>
public sealed class SqliteDb(SqlConnectionInfo connection) : ILogixDb
{
    /// <summary>
    /// Represents the connection information required for interacting with an SQLite database.
    /// This variable contains details such as the database file path, credentials, and other
    /// configuration parameters encapsulated in a <see cref="SqlConnectionInfo"/> instance.
    /// It serves as the primary connection descriptor for database operations.
    /// </summary>
    private readonly SqlConnectionInfo _connection = connection ?? throw new ArgumentNullException(nameof(connection));

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
        new SqliteTagImport()
    ];

    /// <inheritdoc />
    public async Task Migrate(CancellationToken token = default)
    {
        await using var provider = BuildMigrationProvider(_connection.ToConnectionString());
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        await ConfigurePersistentPerformancePragmas(token);
    }

    /// <inheritdoc />
    public async Task Migrate(long version, CancellationToken token = default)
    {
        await using var provider = BuildMigrationProvider(_connection.ToConnectionString());
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        await ConfigurePersistentPerformancePragmas(token);
    }

    /// <inheritdoc />
    public Task Drop(CancellationToken token = default)
    {
        if (File.Exists(_connection.Source))
            File.Delete(_connection.Source);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task Purge(CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        await ExecuteSqlAsync(SqlStatement.DeleteAllTargets, token: token);
    }

    /// <inheritdoc />
    public async Task<IDbConnection> Connect(CancellationToken token = default)
    {
        return await OpenConnectionAsync(token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Snapshot>> ListSnapshots(string? targetKey = null, CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        await using var connection = await OpenConnectionAsync(token);
        var key = new { target_key = targetKey };
        return await connection.QueryAsync<Snapshot>(SqlStatement.ListSnapshots, key);
    }

    /// <inheritdoc />
    public async Task<Snapshot> GetSnapshotLatest(string targetKey, CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        await using var connection = await OpenConnectionAsync(token);
        var key = new { target_key = targetKey };
        return await connection.QuerySingleAsync<Snapshot>(SqlStatement.GetLatestSnapshot, key);
    }

    /// <inheritdoc />
    public async Task<Snapshot> GetSnapshotById(int snapshotId, CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        await using var connection = await OpenConnectionAsync(token);
        var key = new { snapshot_id = snapshotId };
        return await connection.QuerySingleAsync<Snapshot>(SqlStatement.GetSnapshotById, key);
    }

    /// <inheritdoc />
    public async Task AddSnapshot(Snapshot snapshot, SnapshotAction action = SnapshotAction.Append,
        CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        await HandleSnapshotAction(snapshot.TargetKey, action, token);
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

    /// <inheritdoc />
    public async Task DeleteSnapshotsFor(string targetKey, CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        var param = new { target_key = targetKey };
        await ExecuteSqlAsync(SqlStatement.DeleteTargetById, param, token);
    }

    /// <inheritdoc />
    public async Task DeleteSnapshotsBefore(DateTime importDate, string? targetKey = null,
        CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        var param = new { target_key = targetKey, import_date = importDate };
        await ExecuteSqlAsync(SqlStatement.DeleteSnapshotsBefore, param, token);
    }

    /// <inheritdoc />
    public async Task DeleteSnapshotLatest(string targetKey, CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        var param = new { target_key = targetKey };
        await ExecuteSqlAsync(SqlStatement.DeleteSnapshotByLatest, param, token);
    }

    /// <inheritdoc />
    public async Task DeleteSnapshot(int snapshotId, CancellationToken token = default)
    {
        await EnsureCreatedAndMigrated();
        var param = new { snapshot_id = snapshotId };
        await ExecuteSqlAsync(SqlStatement.DeleteSnapshotById, param, token);
    }

    /// <summary>
    /// Handles snapshot actions based on the specified action type for a given target key.
    /// </summary>
    /// <param name="targetKey">The key identifying the target for the snapshot action.</param>
    /// <param name="action">The type of action to perform on the snapshot (Append, ReplaceLatest, or ReplaceAll).</param>
    /// <param name="token">A cancellation token that can be used to signal the operation should be canceled.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified action is not recognized.</exception>
    private async Task HandleSnapshotAction(string targetKey, SnapshotAction action, CancellationToken token)
    {
        switch (action)
        {
            case SnapshotAction.ReplaceLatest:
                await ExecuteSqlAsync(SqlStatement.DeleteSnapshotByLatest, new { target_key = targetKey }, token);
                break;
            case SnapshotAction.ReplaceAll:
                await ExecuteSqlAsync(SqlStatement.DeleteTargetById, new { target_key = targetKey }, token);
                break;
            case SnapshotAction.Append:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    /// <summary>
    /// Executes an SQL statement asynchronously within a transactional context.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">The parameters to bind to the SQL query.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteSqlAsync(string sql, object? param = null, CancellationToken token = default)
    {
        await using var connection = await OpenConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        try
        {
            await connection.ExecuteAsync(sql, param, transaction);
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
    private async Task EnsureCreatedAndMigrated()
    {
        if (!File.Exists(_connection.Source))
            throw new FileNotFoundException($"Database file not found: {_connection.Source}");

        await using var provider = BuildMigrationProvider(_connection.ToConnectionString());
        var runner = provider.GetRequiredService<IMigrationRunner>();

        if (runner.HasMigrationsToApplyUp())
        {
            throw new MigrationRequiredException(_connection.Source);
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
        await using var connection = await OpenConnectionAsync(token);

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
}