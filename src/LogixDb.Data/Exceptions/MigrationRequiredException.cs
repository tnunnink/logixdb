namespace LogixDb.Data.Exceptions;

/// <summary>
/// Exception thrown when a database operation is attempted on a database schema that is outdated
/// and requires migration to the latest version before operations can proceed.
/// </summary>
public class MigrationRequiredException(string dataSource) : Exception(GenerateError(dataSource))
{
    private static string GenerateError(string dataSource)
    {
        return $"The database '{dataSource}' has pending migrations. Please run 'migrate' before using the database.";
    }
}