using LogixDb.Data;
using LogixDb.Data.Abstractions;
using LogixDb.Data.Sqlite;
using LogixDb.Data.SqlServer;
using LogixDb.Service.Configuration;

namespace LogixDb.Service.Common;

/// <summary>
/// Contains extension methods for configuring LogixDb-related services within an application's dependency injection container.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds the LogixDb service to the dependency injection container based on the specified database provider and connection settings in the configuration.
    /// </summary>
    /// <param name="services">The service collection to which the LogixDb service is added.</param>
    /// <param name="config">The configuration options for the service, including database provider and connection details.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified database provider is unsupported.
    /// </exception>
    // ReSharper disable once UnusedMethodReturnValue.Global this is the standard pattern even if not using fluently
    public static IServiceCollection AddLogixDb(this IServiceCollection services, LogixConfig? config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        var connectionString = config.IngestionService.DbConnection;
        var connection = SqlConnectionInfo.Parse(connectionString);

        switch (connection.Provider)
        {
            case SqlProvider.SqlServer:
                services.AddTransient<ILogixDb>(_ => new SqlServerDb(connection));
                break;
            case SqlProvider.Sqlite:
                services.AddTransient<ILogixDb>(_ => new SqliteDb(connection));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(config), connection.Provider, "Unsupported SQL provider");
        }

        return services;
    }

    /// <summary>
    /// Determines whether the specified file name corresponds to a supported Logix file type.
    /// </summary>
    /// <param name="fileName">The name of the file to check, including its extension.</param>
    /// <returns>True if the file extension matches a supported Logix file type; otherwise, false.</returns>
    public static bool IsLogixFile(this string fileName)
    {
        return Enum.TryParse<FileType>(Path.GetExtension(fileName).Trim('.'), out _);
    }
}