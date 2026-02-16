using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents a command to purge all data from the database while preserving the schema.
/// This command removes all records from tables but keeps the database structure intact.
/// </summary>
[PublicAPI]
[Command("purge", Description = "Purges all data from the database while preserving the schema structure")]
public class PurgeCommand : DbCommand
{
    private const string Confirm =
        "Are you sure you want to purge all data from the database? This action cannot be undone.";

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database)
    {
        if (!await console.Ansi().ConfirmAsync(Confirm))
        {
            console.Ansi().MarkupLine("[yellow]Operation cancelled[/]");
            return;
        }

        var cancellation = console.RegisterCancellationHandler();

        try
        {
            await console.Ansi()
                .Status()
                .StartAsync("Purging database...", _ => database.Purge(cancellation));

            console.Ansi().MarkupLine("[green]✓[/] Database purged successfully");
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"Database purge failed with error: {e.Message}",
                ErrorCodes.InternalError,
                false, e
            );
        }
    }
}