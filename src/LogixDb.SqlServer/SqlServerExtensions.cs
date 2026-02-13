using LogixDb.Core.Common;
using Microsoft.Data.SqlClient;

namespace LogixDb.SqlServer;

/// <summary>
/// Provides a collection of utility methods designed to extend the functionality of SQL Server connectivity.
/// </summary>
internal static class SqlServerExtensions
{
    /// <summary>
    /// Converts the given <see cref="SqlConnectionInfo"/> into a connection string for SQL Server.
    /// </summary>
    /// <param name="info">An instance of <see cref="SqlConnectionInfo"/> that contains the connection details.</param>
    /// <param name="database">An optional database name to override the default catalog defined in <paramref name="info"/>.</param>
    /// <returns>A SQL Server connection string based on the provided connection information.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the authentication type specified in <paramref name="info"/> is not supported.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="info"/> is null.
    /// </exception>
    public static string ToConnectionString(this SqlConnectionInfo info, string? database = null)
    {
        ArgumentNullException.ThrowIfNull(info);

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = info.DataSource,
            InitialCatalog = database ?? info.Catalog,
            Pooling = false,
            Encrypt = info.Encrypt,
            TrustServerCertificate = info.Trust
        };

        switch (info.Authentication)
        {
            case SqlAuthentication.SqlServer:
                // SQL Server authentication
                builder.UserID = info.User;
                builder.Password = info.Password;
                builder.IntegratedSecurity = false;
                break;
            case SqlAuthentication.Integrated:
                // Windows Integrated Security
                builder.IntegratedSecurity = true;
                break;
            case SqlAuthentication.ActiveDirectory:
                // Azure Active Directory Integrated
                builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(info), info.Authentication,
                    $"Unsupported authentication method: {info.Authentication}");
        }

        return builder.ToString();
    }
}