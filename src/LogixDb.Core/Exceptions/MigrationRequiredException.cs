namespace LogixDb.Core.Exceptions;

/// <summary>
/// Exception thrown when a database operation is attempted on a database schema that is outdated
/// and requires migration to the latest version before operations can proceed.
/// </summary>
public class MigrationRequiredException() : Exception(Error)
{
    private const string Error =
        "The database file exists but has pending migrations. Please run 'migrate' before using the database.";
}