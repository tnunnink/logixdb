using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Cli.Extensions;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands.Snapshot;

/// <summary>
/// Represents a command to purge snapshots from the database.
/// Can purge all snapshots or snapshots for a specific target.
/// </summary>
[PublicAPI]
[Command("snapshot purge", Description = "Purges snapshots from the database")]
public class SnapshotPurgeCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    [CommandOption("target", 't', Description = "Target key to purge (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    [CommandOption("all", Description = "Purge all snapshots from the database")]
    public bool All { get; init; }

    [CommandOption("force", 'f', Description = "Skip confirmation prompt")]
    public bool Force { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        switch (All)
        {
            case false when string.IsNullOrWhiteSpace(TargetKey):
                throw new CommandException("Either --target or --all must be specified.", ExitCodes.UsageError);
            case true when !string.IsNullOrWhiteSpace(TargetKey):
                throw new CommandException("Cannot specify both --target and --all.", ExitCodes.UsageError);
        }

        var message = All
            ? "This will permanently delete [red]ALL[/] snapshots from the database."
            : $"This will permanently delete all snapshots for target [blue]{TargetKey}[/].";

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
                    await database.Purge();
                else
                    await database.Purge(TargetKey!);
            });

        console.Ansi().MarkupLine("[green]✓[/] Snapshots purged successfully");
    }
}
