namespace LogixDb.Core.Common;

/// <summary>
/// Represents the authentication method to use when connecting to a SQL Server database.
/// This enum defines the available authentication modes for establishing database connections.
/// </summary>
public enum SqlAuthentication
{
    /// <summary>
    /// SQL Server authentication using a username and password.
    /// This mode requires explicit SQL Server login credentials to authenticate.
    /// </summary>
    SqlServer,

    /// <summary>
    /// Windows integrated authentication using the current user's credentials.
    /// This mode leverages Windows security for authentication without requiring an explicit username and password.
    /// </summary>
    Integrated,

    /// <summary>
    /// SQL Server authentication using Azure Active Directory.
    /// This mode allows authentication by leveraging Azure Active Directory accounts
    /// for enhanced security and centralized authentication management.
    /// </summary>
    ActiveDirectory
}