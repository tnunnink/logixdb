using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents a CLI command to execute database migrations. This command ensures that the database schema
/// is up to date with the latest version, creating the schema if it does not already exist.
/// </summary>
/// <remarks>
/// This command is implemented as part of CLI tools for managing and maintaining a database.
/// It handles exceptions during migration and provides relevant feedback to the console regarding
/// the status of the migration process.
/// </remarks>
/// <example>
/// The command can be executed with specific database options such as connection string, provider type,
/// authentication credentials, and other optional parameters inherited from the <c>DbCommand</c> class.
/// </example>
[PublicAPI]
[Command("migrate", Description = "Executes database migrations to ensure the latest schema (creates if non-existent.")]
public class MigrateCommand : DbCommand
{
    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database)
    {
        var cancellation = console.RegisterCancellationHandler();
        
        try
        {
            await console.Ansi()
                .Status()
                .StartAsync("Migrating database...", _ => database.Migrate(cancellation));

            console.Ansi().MarkupLine("[green]✓[/] Database migration completed successfully");
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"Database migration failed with error: {e.Message}",
                ErrorCodes.InternalError,
                false, e
            );
        }
    }
}