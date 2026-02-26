using System.Data;
using LogixDb.Data.Abstractions;
using Microsoft.Data.Sqlite;

namespace LogixDb.Data.Sqlite;

/// <summary>
/// Represents a session for interacting with a SQLite database.
/// This class provides access to the database connection and transaction
/// through strongly typed methods, ensuring type safety and consistency
/// when working with the underlying SQLite infrastructure.
/// </summary>
internal sealed class SqliteDbSession : ILogixDbSession
{
    private readonly SqliteConnection _connection;
    private readonly SqliteTransaction _transaction;

    private SqliteDbSession(SqliteConnection connection, SqliteTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /// <summary>
    /// Asynchronously starts a new database session for interacting with a SQLite database.
    /// Establishes a connection and begins a transaction using the provided database instance.
    /// </summary>
    /// <param name="database">An object implementing <see cref="ILogixDb"/> used to establish the database connection.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an instance of <see cref="SqliteDbSession"/> initialized with a connection and transaction.</returns>
    public static async Task<SqliteDbSession> StartAsync(ILogixDb database, CancellationToken token)
    {
        var connection = (SqliteConnection)await database.Connect(token);
        var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(token);
        return new SqliteDbSession(connection, transaction);
    }

    /// <inheritdoc />
    public TConnection GetConnection<TConnection>() where TConnection : IDbConnection
    {
        if (_connection is not TConnection typed)
            throw new InvalidOperationException(
                $"The requested connection type '{typeof(TConnection).Name}' is not supported. Expected '{nameof(SqliteConnection)}'.");

        return typed;
    }

    /// <inheritdoc />
    public TTransaction GetTransaction<TTransaction>() where TTransaction : IDbTransaction
    {
        if (_transaction is not TTransaction typed)
            throw new InvalidOperationException(
                $"The requested transaction type '{typeof(TTransaction).Name}' is not supported. Expected '{nameof(SqliteTransaction)}'.");

        return typed;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _transaction.DisposeAsync();
    }
}