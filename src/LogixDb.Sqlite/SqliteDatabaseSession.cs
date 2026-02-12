using System.Data;
using LogixDb.Core.Abstractions;
using Microsoft.Data.Sqlite;

namespace LogixDb.Sqlite;

/// <summary>
/// Represents a session for interacting with a SQLite database.
/// This class provides access to the database connection and transaction
/// through strongly typed methods, ensuring type safety and consistency
/// when working with the underlying SQLite infrastructure.
/// </summary>
internal sealed class SqliteDatabaseSession : ILogixDatabaseSession
{
    private readonly SqliteConnection _connection;
    private readonly SqliteTransaction _transaction;

    private SqliteDatabaseSession(SqliteConnection connection, SqliteTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /// <summary>
    /// Asynchronously opens a new instance of <see cref="SqliteDatabaseSession"/> for interacting with the specified SQLite database.
    /// This method initializes a connection and begins a transaction, encapsulating both in a session object.
    /// </summary>
    /// <param name="database">The <see cref="SqliteDatabase"/> instance to open the session for.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the process to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the initialized <see cref="SqliteDatabaseSession"/>.</returns>
    public static async Task<SqliteDatabaseSession> OpenAsync(SqliteDatabase database, CancellationToken token)
    {
        var connection = await database.OpenConnectionAsync(token);
        var transaction = await connection.BeginTransactionAsync(token);
        return new SqliteDatabaseSession(connection, (SqliteTransaction)transaction);
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