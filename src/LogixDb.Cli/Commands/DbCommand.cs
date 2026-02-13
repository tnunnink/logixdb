using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents the base class for CLI commands that interact with a database.
/// This class handles common database-related operations such as configuring
/// connection information and executing commands asynchronously.
/// </summary>
[PublicAPI]
public abstract class DbCommand(ILogixDatabaseFactory factory) : ICommand
{
    /// <summary>
    /// Represents the default name of the database to be used in cases where
    /// a specific database catalog is not explicitly defined in the connection information.
    /// </summary>
    protected const string DefaultDatabaseName = "Logix";

    protected const string DefaultSchemaName = "lgx";

    [CommandOption("connection", 'c', Description = "The database connection string or file path. For SQLite, specify a file path. For SQL Server, use format 'server/database'.")]
    public string? Connection { get; init; }

    [CommandOption("provider", 'p', Description = "The SQL provider type (Sqlite or SqlServer). If not specified, the provider is inferred from the connection string.")]
    public SqlProvider? Provider { get; init; }

    [CommandOption("user", Description = "The username for database authentication (SQL Server only).")]
    public string? User { get; init; }

    [CommandOption("password", Description = "The password for database authentication (SQL Server only).")]
    public string? Password { get; init; }

    [CommandOption("port", Description = "The database server port number (SQL Server only). Default is 1433.")]
    public int Port { get; init; } = 1433;

    [CommandOption("encrypt", Description = "Whether to encrypt the database connection (SQL Server only).")]
    public bool Encrypt { get; init; }

    [CommandOption("trust",
        Description = "Whether to trust the server certificate without validation (SQL Server only).")]
    public bool Trust { get; init; }

    /// <summary>
    /// Executes the asynchronous command logic.
    /// Prepares the necessary SQL connection information based on the input parameters
    /// and delegates execution to an implementation-specific method.
    /// </summary>
    /// <param name="console">The console through which the command interacts with the user.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    public ValueTask ExecuteAsync(IConsole console)
    {
        if (string.IsNullOrWhiteSpace(Connection))
            throw new CommandException("Database argument 'connection' is required.", ErrorCodes.UsageError);

        var provider = Provider ?? InferProvider(Connection);
        var datasource = ParseDataSource(Connection, provider);
        var catalog = ParseCatalog(Connection, provider) ?? DefaultDatabaseName;
        var info = new SqlConnectionInfo(provider, datasource, catalog, User, Password, Port, Encrypt, Trust);
        var database = factory.Create(info);
        return ExecuteAsync(console, database);
    }

    /// <summary>
    /// Executes the command-specific logic using the provided console and database connection.
    /// This method must be implemented by derived classes to define the actual command behavior.
    /// </summary>
    /// <param name="console">The console interface for interacting with the user and displaying output.</param>
    /// <param name="database">The connected Logix database instance to operate on.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    protected abstract ValueTask ExecuteAsync(IConsole console, ILogixDatabase database);

    /// <summary>
    /// Infers the SQL provider type from the database path.
    /// Determines whether the path represents a SQLite database (file-based) or SQL Server (server-based) connection.
    /// </summary>
    /// <param name="databasePath">The database path or connection string to analyze.</param>
    /// <returns>
    /// <see cref="SqlProvider.Sqlite"/> if the path is an existing file or has a file extension;
    /// otherwise, <see cref="SqlProvider.SqlServer"/>.
    /// </returns>
    private static SqlProvider InferProvider(string databasePath)
    {
        // If this is an existing file path, we know it's a Sqlite database.
        if (File.Exists(databasePath)) return SqlProvider.Sqlite;

        // We'll assume that if the provided path has an extension, it should be considered a Sqlite database.
        // Otherwise, we'll assume SqlServer
        return Path.HasExtension(databasePath) ? SqlProvider.Sqlite : SqlProvider.SqlServer;
    }

    /// <summary>
    /// Parses the data source from the provided database path based on the specified SQL provider type.
    /// For SQLite, the database path is returned as-is. For SQL Server, the data source is extracted
    /// from the path, assuming the format contains server and database information separated by a slash.
    /// </summary>
    /// <param name="databasePath">The database path or connection string to analyze.</param>
    /// <param name="provider">The SQL provider type indicating how the data source should be parsed.</param>
    /// <returns>The extracted data source as a string.</returns>
    private static string ParseDataSource(string databasePath, SqlProvider provider)
    {
        if (provider == SqlProvider.Sqlite)
            return databasePath;

        var slash = databasePath.IndexOf('/');
        return slash > 0 ? databasePath[..slash] : databasePath;
    }

    /// <summary>
    /// Extracts the database portion of a given database path, depending on the specified provider.
    /// </summary>
    /// <param name="databasePath">The database path or connection string to analyze.</param>
    /// <param name="provider">The SQL provider type indicating the format of the database path.</param>
    /// <returns>
    /// The database name or schema extracted from the path if the provider is not <see cref="SqlProvider.Sqlite"/>;
    /// otherwise, <c>null</c>.
    /// </returns>
    private static string? ParseCatalog(string databasePath, SqlProvider provider)
    {
        if (provider == SqlProvider.Sqlite)
            return null;

        var slash = databasePath.IndexOf('/');

        if (slash <= 0 || slash == databasePath.Length - 1)
            return null;

        return databasePath[(slash + 1)..];
    }
}