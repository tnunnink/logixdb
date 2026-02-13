using LogixDb.Core.Common;

namespace LogixDb.Core.Abstractions;

/// <summary>
/// Provides factory methods for creating and establishing connections to LogixDB databases.
/// This factory abstracts the creation and initialization of database instances,
/// allowing clients to connect to databases using SQL connection information.
/// </summary>
public interface ILogixDatabaseFactory
{
    /// <summary>
    /// Creates an instance of <see cref="ILogixDatabase"/> based on the provided SQL connection information.
    /// This method is responsible for initializing and configuring a database connection
    /// using the given connection parameters.
    /// </summary>
    /// <param name="connection">The SQL connection information containing details about the provider,
    /// data source, catalog, authentication, port, encryption, and trust settings.</param>
    /// <returns>An instance of <see cref="ILogixDatabase"/> representing the connected and initialized database.</returns>
    ILogixDatabase Create(SqlConnectionInfo connection);
}