using System.Data;
using Dapper;
using FluentMigrator.Runner;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using LogixDb.Core.Exceptions;
using LogixDb.Migrations;
using LogixDb.SqlServer.Imports;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.SqlServer;

/// <summary>
/// Represents a SQL Server database implementation of the ILogixDatabase interface.
/// Provides functionality for database creation, migration, snapshot management,
/// importing, exporting, and purging operations.
/// </summary>
public sealed class SqlServerDb : ILogixDb
{
    /// <summary>
    /// Encapsulates connection-specific information for interacting with a SQL Server database.
    /// This includes details such as data source, catalog, authentication credentials, and other
    /// configuration settings necessary for establishing and managing the database connection.
    /// </summary>
    private readonly SqlConnectionInfo _connection;

    /// <summary>
    /// Provides access to services required for handling database migrations.
    /// This includes the setup and management of migration-related operations,
    /// leveraging dependency injection to resolve instances such as migration runners
    /// responsible for applying schema changes to the SQL Server database.
    /// </summary>
    private readonly ServiceProvider _migrationProvider;

    /// <summary>
    /// Represents the collection of imports used in the Sqlite database implementation.
    /// These imports are responsible for mapping data elements between SQLite and
    /// the application domain, enabling seamless data operations.
    /// </summary>
    private readonly List<ILogixDbImport> _imports =
    [
        new SqlServerSnapshotImport(),
        /*new SqlServerControllerImport(),
        new SqlServerDataTypeImport(),
        new SqlServerDataTypeMemberImport(),
        new SqlServerAoiImport(),
        new SqlServerAoiParameterImport(),
        new SqlServerModuleImport(),
        new SqlServerTaskImport(),
        new SqlServerProgramImport(),
        new SqlServerRoutineImport(),
        new SqlServerRungImport(),*/
        new SqlServerTagImport(),
    ];

    /// <summary>
    /// Represents a SQL Server database implementation of the ILogixDatabase interface.
    /// Provides functionality for database creation, migration, snapshot management,
    /// importing, exporting, and purging operations.
    /// </summary>
    public SqlServerDb(SqlConnectionInfo connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _migrationProvider = BuildMigrationProvider(connection.ToConnectionString());
    }

    /// <inheritdoc />
    public async Task Migrate(CancellationToken token = default)
    {
        await EnsureCreated(token);
        var runner = _migrationProvider.GetRequiredService<IMigrationRunner>();
        await Task.Run(() => runner.MigrateUp(), token);
    }

    /// <inheritdoc />
    public async Task Migrate(long version, CancellationToken token = default)
    {
        await EnsureCreated(token);
        var runner = _migrationProvider.GetRequiredService<IMigrationRunner>();
        await Task.Run(() => runner.MigrateUp(version), token);
    }

    /// <inheritdoc />
    public async Task Drop(CancellationToken token = default)
    {
        await DropDatabase(token);
    }

    /// <inheritdoc />
    public async Task Purge(CancellationToken token = default)
    {
        EnsureMigrated();
        const string sql = "DELETE FROM target WHERE target_id > 0";
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
    public async Task<IDbConnection> Connect(CancellationToken token = default)
    {
        return await OpenConnectionAsync(token);
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
    public async Task AddSnapshot(Snapshot snapshot, CancellationToken token = default)
    {
        EnsureMigrated();
        await using var session = await SqlServerDbSession.StartAsync(OpenConnectionAsync(token), token);

        try
        {
            foreach (var import in _imports)
                await import.Process(snapshot, session, token);

            await session.GetTransaction<SqlTransaction>().CommitAsync(token);
        }
        catch (Exception)
        {
            await session.GetTransaction<SqlTransaction>().RollbackAsync(token);
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
        const string sql = """
                           SELECT TOP 1 snapshot_id [SnapshotId],
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
                           """;
        await using var connection = await OpenConnectionAsync(token);
        var key = new { target_key = snapshotId };
        return await connection.QuerySingleAsync<Snapshot>(sql, key);
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
    /// Ensures that the associated SQL Server database is created. If the database does not exist, it creates it.
    /// </summary>
    /// <param name="token">A cancellation token that can be used to cancel the database creation operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EnsureCreated(CancellationToken token)
    {
        await using var connection = await OpenMasterConnectionAsync(token);

        await connection.ExecuteAsync(
            $"""
             IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = @DatabaseName)
             BEGIN
                 CREATE DATABASE [{_connection.Catalog}]
             END
             """,
            new { DatabaseName = _connection.Catalog }
        );
    }

    /// <summary>
    /// Drops the database specified in the connection information.
    /// Ensures the database is set to single-user mode and removes it from the SQL Server instance.
    /// </summary>
    /// <param name="token">A cancellation token that can be used to observe cancellation requests.</param>
    private async Task DropDatabase(CancellationToken token)
    {
        await using var connection = await OpenMasterConnectionAsync(token);

        await connection.ExecuteAsync(
            $"""
             IF EXISTS (SELECT * FROM sys.databases WHERE name = @DatabaseName)
             BEGIN
               ALTER DATABASE [{_connection.Catalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
               DROP DATABASE [{_connection.Catalog}]
             END
             """,
            new { DatabaseName = _connection.Catalog }
        );
    }

    /// <summary>
    /// Establishes and opens a connection to the master database of the specified SQL Server instance.
    /// </summary>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Returns a <see cref="SqlConnection"/> object representing the opened connection to the master database.</returns>
    private async Task<SqlConnection> OpenMasterConnectionAsync(CancellationToken token)
    {
        var connection = new SqlConnection(_connection.ToConnectionString("master"));
        await connection.OpenAsync(token);
        return connection;
    }

    /// <summary>
    /// Opens and returns a new SQL connection using the configured connection string.
    /// </summary>
    /// <param name="token">The cancellation token to observe while waiting for the connection to open.</param>
    /// <returns>An open <see cref="SqlConnection"/> instance associated with the configured connection string.</returns>
    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken token)
    {
        var connection = new SqlConnection(_connection.ToConnectionString());
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
    /// Creates a service provider configured for running FluentMigrator migrations.
    /// </summary>
    /// <returns>
    /// A <see cref="ServiceProvider"/> configured with migration-specific services that
    /// include SQL Server support and migration scanning in the relevant assemblies.
    /// </returns>
    private static ServiceProvider BuildMigrationProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSqlServer()
                .WithGlobalConnectionString(connectionString)
                .WithVersionTable(new MigrationTableMetaData())
                .ScanIn(
                    typeof(MigrationTableMetaData).Assembly,
                    typeof(SqlServerDb).Assembly)
                .For.Migrations()
            );

        return services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _migrationProvider.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _migrationProvider.DisposeAsync();
    }
}