using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using L5Sharp.Core;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands.Snapshots;

/// <summary>
/// Represents a command to replace an existing snapshot by purging the old one and importing a new one.
/// This is a convenience command that combines purge and import operations.
/// </summary>
[PublicAPI]
[Command("snapshot replace", Description = "Replaces an existing snapshot by purging and importing a new L5X file")]
public class SnapshotReplaceCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    [CommandParameter(1, Name = "file", Description = "Path to the L5X file to import")]
    public string FilePath { get; init; } = string.Empty;

    [CommandOption("target", 't', Description = "Target key (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    [CommandOption("force", 'f', Description = "Skip confirmation prompt")]
    public bool Force { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            throw new CommandException("File path is required.", ErrorCodes.UsageError);

        if (!File.Exists(FilePath))
            throw new CommandException($"File not found: {FilePath}", ErrorCodes.UsageError);

        // Load the snapshot to determine target key
        var content = await L5X.LoadAsync(FilePath);
        var snapshot = Core.Common.Snapshot.Create(content);
        var effectiveTargetKey = TargetKey ?? snapshot.TargetKey;

        // Confirm operation
        if (!Force)
        {
            console.Ansi().MarkupLine(
                $"This will [red]replace[/] all existing snapshots for target [blue]{effectiveTargetKey}[/]."
            );

            if (!await console.Ansi().ConfirmAsync("Are you sure you want to continue?", false))
            {
                console.Ansi().MarkupLine("[yellow]Operation cancelled[/]");
                return;
            }
        }

        Core.Common.Snapshot? importedSnapshot = null;

        await console.Ansi().Status()
            .StartAsync("Replacing snapshot...", async ctx =>
            {
                ctx.Status("Purging existing snapshots...");
                await database.DeleteSnapshot(effectiveTargetKey);

                ctx.Status("Importing new snapshot...");
                importedSnapshot = await database.AddSnapshot(snapshot);
            });

        if (importedSnapshot is not null)
        {
            console.Ansi()
                .MarkupLine($"[green]✓[/] Snapshot replaced successfully (ID: {importedSnapshot.SnapshotId})");
        }
    }
}