using Microsoft.Data.SqlClient;

namespace LogixDb.Data.SqlServer;

/// <summary>
/// Provides a collection of utility methods designed to extend the functionality of SQL Server connectivity.
/// </summary>
public static class SqlServerExtensions
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
            DataSource = $"{info.Source},{info.Port}",
            InitialCatalog = database ?? info.Database,
            Encrypt = info.Encrypt,
            TrustServerCertificate = info.Trust,
            Pooling = false
        };

        if (info.User is not null)
        {
            // SQL Server authentication
            builder.UserID = info.User;
            builder.Password = info.Password;
            builder.IntegratedSecurity = false;
        }
        else
        {
            builder.IntegratedSecurity = true;
        }

        return builder.ToString();
    }
}