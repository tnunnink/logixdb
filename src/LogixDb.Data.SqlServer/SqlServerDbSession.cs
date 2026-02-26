using System.Data;
using LogixDb.Data.Abstractions;
using Microsoft.Data.SqlClient;

namespace LogixDb.Data.SqlServer;

/// <summary>
/// Represents a SQL Server database session that provides access to an open connection and transaction
/// for interacting with a SQL Server database. This class implements the <see cref="ILogixDbSession"/> interface
/// and ensures correct management of the underlying connection and transaction lifecycle.
/// </summary>
public sealed class SqlServerDbSession : ILogixDbSession
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;

    private SqlServerDbSession(SqlConnection connection, SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /// <summary>
    /// Asynchronously starts a new SQL Server database session by establishing a connection
    /// and beginning a database transaction for the specified database.
    /// </summary>
    /// <param name="database">The database instance implementing <see cref="ILogixDb"/> to establish the connection with.</param>
    /// <param name="token">The <see cref="CancellationToken"/> for the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the initialized <see cref="SqlServerDbSession"/>.</returns>
    public static async Task<SqlServerDbSession> StartAsync(ILogixDb database, CancellationToken token)
    {
        var connection = (SqlConnection)await database.Connect(token);
        var transaction = (SqlTransaction)await connection.BeginTransactionAsync(token);
        return new SqlServerDbSession(connection, transaction);
    }


    /// <inheritdoc />
    public TConnection GetConnection<TConnection>() where TConnection : IDbConnection
    {
        if (_connection is not TConnection typed)
            throw new InvalidOperationException(
                $"The requested connection type '{typeof(TConnection).Name}' is not supported. Expected '{nameof(SqlConnection)}'.");

        return typed;
    }

    /// <inheritdoc />
    public TTransaction GetTransaction<TTransaction>() where TTransaction : IDbTransaction
    {
        if (_transaction is not TTransaction typed)
            throw new InvalidOperationException(
                $"The requested transaction type '{typeof(TTransaction).Name}' is not supported. Expected '{nameof(SqlTransaction)}'.");

        return typed;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _transaction.DisposeAsync();
    }
}