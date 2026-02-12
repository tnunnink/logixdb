using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using L5Sharp.Core;
using LogixDb.Cli.Common;
using LogixDb.Cli.Extensions;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands.Snapshot;

/// <summary>
/// Represents a command to import an L5X file as a new snapshot into the database.
/// </summary>
[PublicAPI]
[Command("snapshot import", Description = "Imports an L5X file as a new snapshot into the database")]
public class SnapshotImportCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    [CommandParameter(1, Name = "file", Description = "Path to the L5X file to import")]
    public string FilePath { get; init; } = string.Empty;

    [CommandOption("target", 't', Description = "Optional target key override (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            throw new CommandException("File path is required.", ExitCodes.UsageError);

        if (!File.Exists(FilePath))
            throw new CommandException($"File not found: {FilePath}", ExitCodes.UsageError);

        Core.Common.Snapshot? snapshot = null;

        await AnsiConsole.Status()
            .StartAsync("Importing snapshot...", async ctx =>
            {
                ctx.Status("Loading L5X file...");
                var content = await L5X.LoadAsync(FilePath);
                var snapshotToImport = Core.Common.Snapshot.Create(content);

                ctx.Status("Importing snapshot to database...");
                snapshot = await database.Import(snapshotToImport, TargetKey);
            });

        if (snapshot is not null)
        {
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
}