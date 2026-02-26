namespace LogixDb.Data;

/// <summary>
/// Represents the connection information required to establish a connection
/// with a SQL database. This record encapsulates details about the provider,
/// source, database, authentication credentials, port, and encryption settings.
/// </summary>
public sealed record SqlConnectionInfo(
    SqlProvider Provider,
    string Source,
    string? Database = null,
    string? User = null,
    string? Password = null,
    int Port = 1433,
    bool Encrypt = false,
    bool Trust = false
)
{
    /// <summary>
    /// Represents the ampersand character ('@'), used internally within the <c>SqlConnectionInfo</c> class
    /// to identify and parse SQL connection strings.
    /// The character is used to determine the boundary between the catalog (database name)
    /// and data source (server/host information) in SQL Server connection strings.
    /// </summary>
    private const char Ampersand = '@';

    /// <summary>
    /// Represents the semicolon character (';'), used within the <c>SqlConnectionInfo</c> class
    /// to delimit individual key-value pairs in SQL connection strings.
    /// This character separates configuration options such as data source, catalog, user, and other parameters.
    /// </summary>
    private const char SemiColon = ';';

    /// <summary>
    /// Represents the equals character ('='), used internally within the <c>SqlConnectionInfo</c>
    /// class to parse key-value pairs in SQL connection strings.
    /// It serves as a delimiter between keys (e.g., "User") and their corresponding values
    /// (e.g., "admin") in the connection string format.
    /// </summary>
    private const char Equal = '=';

    /// <summary>
    /// Parses a connection string and constructs an instance of <c>SqlConnectionInfo</c>.
    /// Determines the SQL provider, data source, and catalog from the provided connection string
    /// and populates the corresponding properties in the returned object.
    /// </summary>
    /// <param name="connection">The connection string to parse, containing information such as
    /// the data source, database catalog, and potentially other configuration details.</param>
    /// <returns>A new instance of <c>SqlConnectionInfo</c> populated with the parsed connection details.</returns>
    /// <exception cref="ArgumentException">Thrown when the SQL provider cannot be inferred
    /// from the connection string.</exception>
    public static SqlConnectionInfo Parse(string connection)
    {
        var provider = InferProvider(connection);

        if (provider == SqlProvider.Sqlite)
            return new SqlConnectionInfo(provider, connection);

        var parts = connection.Split(SemiColon);
        var dataSource = parts[0];
        var atIndex = dataSource.IndexOf(Ampersand);
        var catalog = dataSource[..atIndex];
        var datasource = dataSource[(atIndex + 1)..];

        var info = new SqlConnectionInfo(provider, datasource, catalog);

        for (var i = 1; i < parts.Length; i++)
        {
            var pair = parts[i].Split(Equal);

            if (pair.Length != 2)
                throw new FormatException(
                    $"Invalid connection string format. Expected 'Key=Value' but got '{parts[i]}'.");

            var key = pair[0];
            var value = pair[1];

            info = key switch
            {
                nameof(User) => info with { User = value },
                nameof(Password) => info with { Password = value },
                nameof(Port) => info with { Port = int.Parse(value) },
                nameof(Trust) => info with { Trust = bool.Parse(value) },
                nameof(Encrypt) => info with { Encrypt = bool.Parse(value) },
                _ => info
            };
        }

        return info;
    }

    /// <summary>
    /// Infers the SQL provider type based on the given database path or connection string.
    /// Determines whether the path corresponds to a SQL Server connection or an SQLite file,
    /// based on the presence of specific separators, file existence, or file name patterns.
    /// </summary>
    /// <param name="connectionString">The database path or connection string to analyze.</param>
    /// <returns>The inferred SQL provider type as an <c>SqlProvider</c> enum value.</returns>
    /// <exception cref="ArgumentException">Thrown if the SQL provider cannot be inferred from the provided database path.</exception>
    private static SqlProvider InferProvider(string connectionString)
    {
        // If the path contains the host separator, we know it is SqlServer.
        if (connectionString.IndexOf(Ampersand) > 0)
            return SqlProvider.SqlServer;

        // If this is an existing file, or it appears to be a valid file name, assume a Sqlite provider.
        if (File.Exists(connectionString) || IsFileName(connectionString))
            return SqlProvider.Sqlite;

        throw new ArgumentException($"Unable to infer SQL provider from connection string '{connectionString}')");

        bool IsFileName(string text)
        {
            return !string.IsNullOrWhiteSpace(Path.GetFileName(text)) && Path.HasExtension(connectionString);
        }
    }
}