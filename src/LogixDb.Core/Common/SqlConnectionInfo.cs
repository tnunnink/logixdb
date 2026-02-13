namespace LogixDb.Core.Common;

/// <summary>
/// Represents the connection information required to connect to a SQL database.
/// This class is a record type, meaning it is immutable and uses value semantics.
/// </summary>
public sealed record SqlConnectionInfo(
    SqlProvider Provider,
    string DataSource,
    string Catalog = "logix",
    SqlAuthentication Authentication = SqlAuthentication.Integrated,
    string? User = null,
    string? Password = null,
    int Port = 1433,
    bool Encrypt = false,
    bool Trust = false
);