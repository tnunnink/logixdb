using System.Data;

namespace LogixDb.Data.Abstractions;

/// <summary>
/// Represents a database session abstraction that provides access to database connections and transactions.
/// This interface allows callers to retrieve strongly typed connection and transaction instances
/// for interacting with the underlying database within a session context.
/// </summary>
public interface ILogixDbSession : IAsyncDisposable
{
    /// <summary>
    /// Retrieves the database connection associated with this session.
    /// </summary>
    /// <typeparam name="TConnection">The type of database connection to retrieve, which must implement <see cref="IDbConnection"/>.</typeparam>
    /// <returns>An instance of the specified connection type representing the active database connection.</returns>
    TConnection GetConnection<TConnection>() where TConnection : IDbConnection;

    /// <summary>
    /// Retrieves the database transaction associated with this session.
    /// </summary>
    /// <typeparam name="TTransaction">The type of database transaction to retrieve, which must implement <see cref="IDbTransaction"/>.</typeparam>
    /// <returns>An instance of the specified transaction type representing the active database transaction.</returns>
    TTransaction GetTransaction<TTransaction>() where TTransaction : IDbTransaction;
}