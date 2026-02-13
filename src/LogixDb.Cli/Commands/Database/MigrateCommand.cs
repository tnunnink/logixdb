using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands.Database;

/// <summary>
/// Represents a command to perform database migration operations using the LogixDB system.
/// This command facilitates the migration of database schemas or data structures
/// to ensure compatibility with updated application requirements.
/// </summary>
[PublicAPI]
[Command("migrate", Description = "Executes database migrations to ensure the database structure is up to date")]
public class MigrateCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    /// <inheritdoc />
    protected override ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        console.Ansi().Status().Start("Migrating database...", _ => { database.Migrate(); });
        console.Ansi().MarkupLine("[green]✓[/] Database migration completed successfully");
        return ValueTask.CompletedTask;
    }
}