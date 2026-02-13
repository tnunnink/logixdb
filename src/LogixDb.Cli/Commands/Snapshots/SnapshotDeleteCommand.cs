using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands.Snapshots;

/// <summary>
/// Represents a command to purge snapshots from the database.
/// Can purge all snapshots or snapshots for a specific target.
/// </summary>
[PublicAPI]
[Command("snapshot delete", Description = "")]
public class SnapshotDeleteCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    [CommandOption("target", 't', Description = "Target key to purge (format: targettype://targetname)")]
    public string? Target { get; init; }

    [CommandOption("all", Description = "Purge all snapshots from the database")]
    public bool All { get; init; }

    [CommandOption("force", 'f', Description = "Skip confirmation prompt")]
    public bool Force { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        switch (All)
        {
            case false when string.IsNullOrWhiteSpace(Target):
                throw new CommandException("Either --target or --all must be specified.", ErrorCodes.UsageError);
            case true when !string.IsNullOrWhiteSpace(Target):
                throw new CommandException("Cannot specify both --target and --all.", ErrorCodes.UsageError);
        }

        var message = All
            ? "This will permanently delete [red]ALL[/] snapshots from the database."
            : $"This will permanently delete all snapshots for target [blue]{Target}[/].";

        if (!Force)
        {
            console.Ansi().MarkupLine(message);
            if (!await console.Ansi().ConfirmAsync("Are you sure you want to continue?", false))
            {
                console.Ansi().MarkupLine("[yellow]Operation cancelled[/]");
                return;
            }
        }

        await console.Ansi().Status()
            .StartAsync("Purging snapshots...", async _ =>
            {
                if (All)
                    await database.PurgeSnapshots();
                else
                    await database.DeleteSnapshot(Target!);
            });

        console.Ansi().MarkupLine("[green]✓[/] Snapshots purged successfully");
    }
}
