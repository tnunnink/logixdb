namespace LogixDb.Core;

/// <summary>
/// Represents the connection information required to connect to a SQL database.
/// This class is a record type, meaning it is immutable and uses value semantics.
/// </summary>
public sealed record SqlConnectionInfo(
    SqlProvider Provider,
    string DataSource,
    string Catalog,
    string Authentication,
    int Port,
    bool Encrypt,
    bool Trust
);