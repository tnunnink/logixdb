using Dapper;
using FluentMigrator.Runner;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using LogixDb.Core.Exceptions;
using LogixDb.Migrations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.SqlServer;

/// <summary>
/// Represents a SQL Server database implementation of the ILogixDatabase interface.
/// Provides functionality for database creation, migration, snapshot management,
/// importing, exporting, and purging operations.
/// </summary>
public class SqlServerDatabase(SqlConnectionInfo info, IEnumerable<ILogixDatabaseImport> imports) : ILogixDatabase
{
    /// <inheritdoc />
    public async Task Build(bool recreate = false, CancellationToken token = default)
    {
        // Drop the database if recreate is requested
        if (recreate) await DropDatabase(token);

        // Exit early if the database exists.
        if (await DatabaseExists(token)) return;

        // Otherwise, create the database on the server.
        await CreateDatabase(token);

        // Register and resolve the local migration runner to perform migration.
        await using var provider = BuildMigrationProvider();
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();

        // Migrate the database instance that was created to the latest version.
        runner.MigrateUp();
    }

    /// <inheritdoc />
    public void Migrate()
    {
        using var provider = BuildMigrationProvider();
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    public async Task<IEnumerable<Snapshot>> Snapshots(string? targetKey = null, CancellationToken token = default)
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

    public Task<Snapshot> Import(Snapshot snapshot, string? targetKey = null, CancellationToken token = default)
    {
        EnsureMigrated();
        throw new NotImplementedException();
    }

    public Task<Snapshot> Export(string targetKey, CancellationToken token = default)
    {
        EnsureMigrated();
        throw new NotImplementedException();
    }

    public Task Purge(CancellationToken token = default)
    {
        EnsureMigrated();
        throw new NotImplementedException();
    }

    public Task Purge(string targetKey, CancellationToken token = default)
    {
        EnsureMigrated();
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks whether the specified database exists on the SQL Server instance.
    /// </summary>
    /// <param name="token">A cancellation token to observe while checking the database existence.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the database exists.</returns>
    private async Task<bool> DatabaseExists(CancellationToken token)
    {
        await using var connection = await OpenMasterConnectionAsync(token);

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName",
            new { DatabaseName = info.Catalog }
        );

        return count > 0;
    }

    /// <summary>
    /// Creates a new SQL Server database using the provided connection information.
    /// </summary>
    /// <param name="token">The cancellation token to observe while waiting for the operation to complete.</param>
    private async Task CreateDatabase(CancellationToken token)
    {
        await using var connection = await OpenMasterConnectionAsync(token);
        await connection.ExecuteAsync($"CREATE DATABASE [{info.Catalog}]");
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
             IF EXISTS (SELECT * FROM sys.databases WHERE name = '{info.Catalog}')
             BEGIN
                 ALTER DATABASE [{info.Catalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                 DROP DATABASE [{info.Catalog}]
             END
             """);
    }

    /// <summary>
    /// Establishes and opens a connection to the master database of the specified SQL Server instance.
    /// </summary>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Returns a <see cref="SqlConnection"/> object representing the opened connection to the master database.</returns>
    private async Task<SqlConnection> OpenMasterConnectionAsync(CancellationToken token)
    {
        var connectionString = info.ToConnectionString("master");
        var connection = new SqlConnection(connectionString);
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
        var connectionString = info.ToConnectionString();
        var connection = new SqlConnection(connectionString);
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
        using var provider = BuildMigrationProvider();
        using var scope = provider.CreateScope();
        var runner = provider.GetRequiredService<IMigrationRunner>();

        if (runner.HasMigrationsToApplyUp())
            throw new MigrationRequiredException();
    }

    /// <summary>
    /// Creates a service provider configured for running FluentMigrator migrations.
    /// </summary>
    /// <returns>
    /// A <see cref="ServiceProvider"/> configured with migration-specific services that
    /// include SQL Server support and migration scanning in the relevant assemblies.
    /// </returns>
    private ServiceProvider BuildMigrationProvider()
    {
        var connectionString = info.ToConnectionString();

        var services = new ServiceCollection();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSqlServer()
                .WithGlobalConnectionString(connectionString)
                .WithVersionTable(new MigrationTableMetaData())
                .ScanIn(
                    typeof(MigrationTableMetaData).Assembly,
                    typeof(SqlServerDatabase).Assembly)
                .For.Migrations()
            );

        return services.BuildServiceProvider();
    }
}