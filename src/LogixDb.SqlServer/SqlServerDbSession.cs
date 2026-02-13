using System.Data;
using LogixDb.Core.Abstractions;
using Microsoft.Data.SqlClient;

namespace LogixDb.SqlServer;

/// <summary>
/// Represents a SQL Server database session that provides access to an open connection and transaction
/// for interacting with a SQL Server database. This class implements the <see cref="ILogixDatabaseSession"/> interface
/// and ensures correct management of the underlying connection and transaction lifecycle.
/// </summary>
public sealed class SqlServerDbSession : ILogixDatabaseSession
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;

    private SqlServerDbSession(SqlConnection connection, SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="open"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<SqlServerDbSession> StartAsync(Task<SqlConnection> open, CancellationToken token)
    {
        var connection = await open;
        var transaction = await connection.BeginTransactionAsync(token);
        return new SqlServerDbSession(connection, (SqlTransaction)transaction);
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