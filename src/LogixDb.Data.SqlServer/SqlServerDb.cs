using System.Data;
using System.Data.Common;
using Dapper;
using FluentMigrator.Runner;
using LogixDb.Data.Abstractions;
using LogixDb.Data.Exceptions;
using LogixDb.Data.SqlServer.Imports;
using LogixDb.Migrations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Data.SqlServer;

/// <summary>
/// Represents a SQL Server database implementation of the ILogixDatabase interface.
/// Provides functionality for database creation, migration, snapshot management,
/// importing, exporting, and purging operations.
/// </summary>
public sealed class SqlServerDb(SqlConnectionInfo connection) : ILogixDb
{
    /// <summary>
    /// Encapsulates connection-specific information for interacting with a SQL Server database.
    /// This includes details such as data source, catalog, authentication credentials, and other
    /// configuration settings necessary for establishing and managing the database connection.
    /// </summary>
    private readonly SqlConnectionInfo _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <summary>
    /// Represents the collection of imports used in the SqlServer database implementation.
    /// These imports are responsible for mapping data elements between SqlServer and
    /// the application domain, enabling seamless data operations.
    /// </summary>
    private readonly List<ILogixDbImport> _imports =
    [
        new SqlServerSnapshotImport(),
        new SqlServerControllerImport(),
        new SqlServerDataTypeImport(),
        new SqlServerDataTypeMemberImport(),
        new SqlServerAoiImport(),
        new SqlServerAoiParameterImport(),
        new SqlServerModuleImport(),
        new SqlServerTaskImport(),
        new SqlServerProgramImport(),
        new SqlServerRoutineImport(),
        new SqlServerRungImport(),
        new SqlServerTagImport()
    ];

    /// <inheritdoc />
    public async Task Migrate(CancellationToken token = default)
    {
        await EnsureCreated(token);
        await using var provider = BuildMigrationProvider(_connection.ToConnectionString());
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    /// <inheritdoc />
    public async Task Migrate(long version, CancellationToken token = default)
    {
        await EnsureCreated(token);
        await using var provider = BuildMigrationProvider(_connection.ToConnectionString());
        var runner = provider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    /// <inheritdoc />
    public async Task Drop(CancellationToken token = default)
    {
        await using var connection = await OpenMasterConnectionAsync(token);

        await connection.ExecuteAsync(
            $"""
             IF EXISTS (SELECT * FROM sys.databases WHERE name = @DatabaseName)
             BEGIN
               ALTER DATABASE [{_connection.Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
               DROP DATABASE [{_connection.Database}]
             END
             """,
            new { DatabaseName = _connection.Database }
        );
    }

    /// <inheritdoc />
    public async Task Purge(CancellationToken token = default)
    {
        await ValidateMigration(token);
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
        await ValidateMigration(token);
        await using var connection = await OpenConnectionAsync(token);
        var key = new { target_key = targetKey };
        return await connection.QueryAsync<Snapshot>(SqlStatement.ListSnapshots, key);
    }

    /// <inheritdoc />
    public async Task<Snapshot> GetSnapshotLatest(string targetKey, CancellationToken token = default)
    {
        await ValidateMigration(token);
        await using var connection = await OpenConnectionAsync(token);
        var key = new { target_key = targetKey };
        return await connection.QuerySingleAsync<Snapshot>(SqlStatement.GetLatestSnapshot, key);
    }

    /// <inheritdoc />
    public async Task<Snapshot> GetSnapshotById(int snapshotId, CancellationToken token = default)
    {
        await ValidateMigration(token);
        await using var connection = await OpenConnectionAsync(token);
        var key = new { snapshot_id = snapshotId };
        return await connection.QuerySingleAsync<Snapshot>(SqlStatement.GetSnapshotById, key);
    }

    public async Task AddSnapshot(Snapshot snapshot, SnapshotAction action = SnapshotAction.Append,
        CancellationToken token = default)
    {
        await ValidateMigration(token);
        await HandleSnapshotAction(snapshot.TargetKey, action, token);
        await using var session = await SqlServerDbSession.StartAsync(this, token);

        try
        {
            foreach (var import in _imports)
                await import.Process(snapshot, session, token);

            await session.GetTransaction<DbTransaction>().CommitAsync(token);
        }
        catch (Exception)
        {
            await session.GetTransaction<DbTransaction>().RollbackAsync(token);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteSnapshotsFor(string targetKey, CancellationToken token = default)
    {
        await ValidateMigration(token);
        var param = new { target_key = targetKey };
        await ExecuteSqlAsync(SqlStatement.DeleteTargetById, param, token);
    }

    /// <inheritdoc />
    public async Task DeleteSnapshotsBefore(DateTime importDate, string? targetKey = null,
        CancellationToken token = default)
    {
        await ValidateMigration(token);
        var param = new { target_key = targetKey, import_date = importDate };
        await ExecuteSqlAsync(SqlStatement.DeleteSnapshotsBefore, param, token);
    }

    /// <inheritdoc />
    public async Task DeleteSnapshotLatest(string targetKey, CancellationToken token = default)
    {
        await ValidateMigration(token);
        var param = new { target_key = targetKey };
        await ExecuteSqlAsync(SqlStatement.DeleteSnapshotByLatest, param, token);
    }

    /// <inheritdoc />
    public async Task DeleteSnapshot(int snapshotId, CancellationToken token = default)
    {
        await ValidateMigration(token);
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
    /// Validates connection to the database and then ensures that all pending migrations
    /// have been applied to the database.
    /// </summary>
    private async Task ValidateMigration(CancellationToken token = default)
    {
        try
        {
            await OpenConnectionAsync(token);
        }
        catch (SqlException e)
        {
            throw new InvalidOperationException(
                $"Failed to connect to database with error {e.Message}. Ensure migration by running the 'migrate' command.");
        }

        await using var provider = BuildMigrationProvider(_connection.ToConnectionString());
        var runner = provider.GetRequiredService<IMigrationRunner>();

        if (runner.HasMigrationsToApplyUp())
        {
            throw new MigrationRequiredException(_connection.Source);
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
                 CREATE DATABASE [{_connection.Database}]
             END
             """,
            new { DatabaseName = _connection.Database }
        );
    }

    /// <summary>
    /// Establishes and opens a connection to the master database of the specified SQL Server instance.
    /// </summary>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Returns a <see cref="SqlConnection"/> object representing the opened connection to the master database.</returns>
    private async Task<SqlConnection> OpenMasterConnectionAsync(CancellationToken token)
    {
        var connectionString = _connection.ToConnectionString("master");
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
        var connection = new SqlConnection(_connection.ToConnectionString());
        await connection.OpenAsync(token);
        return connection;
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
}