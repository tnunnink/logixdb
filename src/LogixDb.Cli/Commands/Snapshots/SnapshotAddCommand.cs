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
public class SnapshotAddCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    [CommandOption("source", 's', Description = "Path to the source L5X file to add")]
    public string? SourcePath { get; init; }

    [CommandOption("key", 'k', Description = "Optional target key override (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new CommandException("File path is required.", ErrorCodes.UsageError);

        if (!File.Exists(SourcePath))
            throw new CommandException($"File not found: {SourcePath}", ErrorCodes.FileNotFound);

        var snapshot = await console.Ansi()
            .Status()
            .StartAsync("Importing source...", async ctx =>
            {
                ctx.Status("Loading L5X file...");
                var content = await L5X.LoadAsync(SourcePath);
                ctx.Status("Importing source to database...");
                return await database.AddSnapshot(Snapshot.Create(content, TargetKey));
            });

        var table = new Table().Border(TableBorder.Rounded).AddColumn("Property").AddColumn("Value");
        table.AddRow("Snapshot ID", snapshot.SnapshotId.ToString());
        table.AddRow("Target Key", snapshot.TargetKey);
        table.AddRow("Target Type", snapshot.TargetType);
        table.AddRow("Target Name", snapshot.TargetName);
        table.AddRow("Import Date", snapshot.ImportDate.ToString("yyyy-MM-dd HH:mm:ss"));

        console.Ansi().MarkupLine("[green]✓[/] Snapshot imported successfully");
        console.Ansi().Write(table);
    }
}