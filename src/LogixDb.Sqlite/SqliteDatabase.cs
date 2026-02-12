using Dapper;
using FluentMigrator.Runner;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
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
    private readonly string _connectionString = BuildConnectionString(info);
    
    private const string ListAllSnapshots =
        """
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
        """;

    private const string ListSnapshotsForTarget =
        """
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
        WHERE target_id = (SELECT target_id FROM target where target_key = @target_key)
        """;

    /// <inheritdoc />
    public ILogixDatabase Build()
    {
        using var provider = BuildServiceProvider(_connectionString);
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();

        if (File.Exists(info.DataSource))
        {
            return runner.HasMigrationsToApplyUp()
                ? throw new InvalidOperationException(
                    "The database file exists but has pending migrations. Please run migrations before using the database.")
                : this;
        }

        runner.MigrateUp();
        return this;
    }

    /// <inheritdoc />
    public void Migrate()
    {
        using var provider = BuildServiceProvider(_connectionString);
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Snapshot>> Snapshots(string? targetKey = null, CancellationToken token = default)
    {
        await using var connection = await OpenConnectionAsync(token);

        if (string.IsNullOrEmpty(targetKey))
        {
            return await connection.QueryAsync<Snapshot>(ListAllSnapshots);
        }

        return await connection.QueryAsync<Snapshot>(ListSnapshotsForTarget, targetKey);
    }

    /// <inheritdoc />
    public async Task<Snapshot> Import(Snapshot snapshot, string? targetKey = null, CancellationToken token = default)
    {
        await using var session = await SqliteDatabaseSession.OpenAsync(this, token);

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

    public Task<Snapshot> Export(string targetKey, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task Purge(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task Purge(string targetKey, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Establishes and opens an asynchronous connection to the SQLite database using the configured connection string.
    /// </summary>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an open database connection.</returns>
    public async Task<SqliteConnection> OpenConnectionAsync(CancellationToken token)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(token);
        return connection;
    }

    /// <summary>
    /// Builds and configures a service provider with FluentMigrator services for database migrations.
    /// </summary>
    /// <param name="connectionString">The connection string to use for database migrations.</param>
    /// <returns>A configured <see cref="ServiceProvider"/> instance with FluentMigrator services registered.</returns>
    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(
                    typeof(MigrationTableMetaData).Assembly,
                    typeof(SqliteDatabase).Assembly)
                .For.Migrations()
            );

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Constructs a SQLite connection string based on the provided connection information.
    /// </summary>
    /// <param name="info">An instance of <see cref="SqlConnectionInfo"/> containing the necessary details to construct the connection string.</param>
    /// <returns>A string representing the SQLite connection string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
    private static string BuildConnectionString(SqlConnectionInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = info.DataSource,
            ForeignKeys = true,
            Pooling = false
        };

        return builder.ConnectionString;
    }
}