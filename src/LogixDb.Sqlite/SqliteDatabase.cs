using Dapper;
using FluentMigrator.Runner;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using LogixDb.Core.Exceptions;
using LogixDb.Migrations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Sqlite;

/// <summary>
/// Represents an SQLite-backed implementation of the <see cref="ILogixDatabase"/> interface.
/// This class provides methods to manage database migrations, snapshots, and data import/export processes
/// within an SQLite database.
/// </summary>
internal class SqliteDatabase(SqlConnectionInfo info, IEnumerable<ILogixDatabaseImport> imports) : ILogixDatabase
{
    private readonly SqlConnectionInfo _info = info ?? throw new ArgumentNullException(nameof(info));

    /// <inheritdoc />
    public async Task Build(bool rebuild = false, CancellationToken token = default)
    {
        if (rebuild && File.Exists(_info.DataSource))
            File.Delete(_info.DataSource);

        if (File.Exists(_info.DataSource))
            return;

        var connectionString = _info.ToConnectionString();
        await using var provider = BuildMigrationProvider(connectionString);
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();

        // For Sqlite the migration call will also create the database file.
        await Task.Run(() => runner.MigrateUp(), token);

        // After migration of the database enable performance enhancing PRAGMA settings.
        await ConfigurePersistentPerformancePragmas(token);
    }

    /// <inheritdoc />
    public void Migrate()
    {
        var connectionString = _info.ToConnectionString();
        using var provider = BuildMigrationProvider(connectionString);
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Snapshot>> ListSnapshots(string? targetKey = null, CancellationToken token = default)
    {
        EnsureMigrated();
        const string sql = """
                           SELECT snapshot_id [SnapshotId],
                                 target_id [TargetId],
                                 target_type [TargetType],
                                 target_name [TargetName],
                                 is_partial [IsPartial],
                                 schema_revision [SchemaRevision],
                                 software_revision [SoftwareRevision],
                                 export_date [ExportDate],
                                 export_options [ExportOptions],
                                 import_date [ImportDate],
                                 import_user [ImportUser],
                                 import_machine [ImportMachine],
                                 source_hash [ImportHash] 
                           FROM snapshot
                           WHERE @target_key is null or target_id = (SELECT target_id FROM target where target_key = @target_key)
                           """;
        await using var connection = await OpenConnectionAsync(token);
        var key = new { target_key = targetKey };
        return await connection.QueryAsync<Snapshot>(sql, key);
    }

    /// <inheritdoc />
    public async Task<Snapshot> AddSnapshot(Snapshot snapshot, CancellationToken token = default)
    {
        EnsureMigrated();
        await using var session = await SqliteDatabaseSession.StartAsync(OpenConnectionAsync(token), token);

        try
        {
            foreach (var import in imports)
                await import.Process(snapshot, session, token);

            await session.GetTransaction<SqliteTransaction>().CommitAsync(token);
            return snapshot;
        }
        catch (Exception)
        {
            await session.GetTransaction<SqliteTransaction>().RollbackAsync(token);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Snapshot> GetSnapshot(string targetKey, CancellationToken token = default)
    {
        EnsureMigrated();
        const string sql = """
                           SELECT snapshot_id [SnapshotId],
                                 target_id [TargetId],
                                 target_type [TargetType],
                                 target_name [TargetName],
                                 is_partial [IsPartial],
                                 schema_revision [SchemaRevision],
                                 software_revision [SoftwareRevision],
                                 export_date [ExportDate],
                                 export_options [ExportOptions],
                                 import_date [ImportDate],
                                 import_user [ImportUser],
                                 import_machine [ImportMachine],
                                 source_hash [SourceHash], 
                                 source_data [SourceData] 
                           FROM snapshot
                           WHERE target_id = (SELECT target_id FROM target where target_key = @target_key)
                           ORDER BY import_date DESC
                           LIMIT 1
                           """;
        await using var connection = await OpenConnectionAsync(token);
        var key = new { target_key = targetKey };
        return await connection.QuerySingleAsync<Snapshot>(sql, key);
    }

    /// <inheritdoc />
    public async Task PurgeSnapshots(CancellationToken token = default)
    {
        EnsureMigrated();
        const string sql = "DELETE FROM main.snapshot WHERE snapshot_id > 0";
        await using var connection = await OpenConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);

        try
        {
            await connection.ExecuteAsync(sql, transaction);
            await transaction.CommitAsync(token);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteSnapshot(string targetKey, CancellationToken token = default)
    {
        EnsureMigrated();
        const string sql = """
                           DELETE FROM main.snapshot 
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

    /// <summary>
    /// Establishes and opens an asynchronous connection to the SQLite database using the configured connection string.
    /// </summary>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an open database connection.</returns>
    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken token)
    {
        var connectionString = _info.ToConnectionString();
        var connection = new SqliteConnection(connectionString);
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
        var connectionString = _info.ToConnectionString();
        using var provider = BuildMigrationProvider(connectionString);
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();

        if (runner.HasMigrationsToApplyUp())
            throw new MigrationRequiredException();
    }

    /// <summary>
    /// Builds and configures a service provider with FluentMigrator services for database migrations.
    /// </summary>
    /// <param name="connectionString">The connection string to use for database migrations.</param>
    /// <returns>A configured <see cref="ServiceProvider"/> instance with FluentMigrator services registered.</returns>
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
                    typeof(SqliteDatabase).Assembly)
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
        var connectionString = _info.ToConnectionString();
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(token);
        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            PRAGMA journal_mode = WAL;
            PRAGMA auto_vacuum = INCREMENTAL;
            PRAGMA temp_store = MEMORY;
            PRAGMA busy_timeout = 5000;
            PRAGMA page_size = 16384;
            """;

        await command.ExecuteNonQueryAsync(token);
    }
}