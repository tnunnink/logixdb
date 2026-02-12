using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using LogixDb.Sqlite.Imports;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Sqlite;

/// <summary>
/// Provides methods for registering services to enable LogixDb functionality
/// with SQLite databases. This class contains extension methods for configuring
/// the necessary dependencies required for database interaction and import
/// operations specific to SQLite.
/// </summary>
public static class SqliteRegistration
{
    /// <summary>
    /// Registers services required for LogixDb to work with SQLite databases, including
    /// the database factory and import implementations for specific SQLite operations.
    /// </summary>
    /// <param name="services">The service collection to which the LogixDb SQLite services will be added.</param>
    /// <returns>The updated service collection with registered LogixDb SQLite services.</returns>
    public static IServiceCollection AddLogixSqlite(this IServiceCollection services)
    {
        services.AddTransient<ILogixDatabaseImport, SqliteSnapshotImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteControllerImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteDataTypeImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteDataTypeMemberImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteAoiImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteAoiParameterImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteModuleImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteTaskImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteProgramImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteRoutineImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteRungImport>();
        services.AddTransient<ILogixDatabaseImport, SqliteTagImport>();
        services.AddTransient<SqliteDatabaseFactory>();
        return services;
    }
}