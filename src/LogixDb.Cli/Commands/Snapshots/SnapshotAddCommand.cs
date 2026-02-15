using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using L5Sharp.Core;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;
using Snapshot = LogixDb.Core.Common.Snapshot;

namespace LogixDb.Cli.Commands.Snapshots;

/// <summary>
/// Represents a command to import an L5X file as a new snapshot into the database.
/// </summary>
[PublicAPI]
[Command("snapshot add", Description = "Imports an L5X file as a new snapshot into the database")]
public class SnapshotAddCommand : DbCommand
{
    [CommandOption("source", 's', Description = "Path to the source L5X file to add")]
    public string? SourcePath { get; init; }

    [CommandOption("key", 'k', Description = "Optional target key override (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database)
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new CommandException("File path is required.", ErrorCodes.UsageError);

        if (!File.Exists(SourcePath))
            throw new CommandException($"File not found: {SourcePath}", ErrorCodes.FileNotFound);

        var result = await console.Ansi()
            .Status()
            .StartAsync("Importing source...", async ctx =>
            {
                ctx.Status("Loading L5X file...");
                var content = await L5X.LoadAsync(SourcePath);
                var snapshot = Snapshot.Create(content, TargetKey);
                ctx.Status("Importing source to database...");
                await database.AddSnapshot(snapshot);
                return snapshot;
            });

        var table = new Table().Border(TableBorder.Rounded).AddColumn("Property").AddColumn("Value");
        table.AddRow("Snapshot ID", result.SnapshotId.ToString());
        table.AddRow("Target Key", result.TargetKey);
        table.AddRow("Target Type", result.TargetType);
        table.AddRow("Target Name", result.TargetName);
        table.AddRow("Import Date", result.ImportDate.ToString("yyyy-MM-dd HH:mm:ss"));

        console.Ansi().MarkupLine("[green]✓[/] Snapshot imported successfully");
        console.Ansi().Write(table);
    }
}