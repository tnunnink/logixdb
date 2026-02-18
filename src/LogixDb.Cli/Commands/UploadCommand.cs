using System.Xml;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using L5Sharp.Core;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using Spectre.Console;
using Snapshot = LogixDb.Core.Common.Snapshot;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents a command to import an L5X file as a new snapshot into the database.
/// </summary>
[PublicAPI]
[Command("upload", Description = "Imports an L5X file as a new snapshot into the database")]
public class UploadCommand : DbCommand
{
    [CommandOption("source", 's', IsRequired = true, Description = "Path to the source L5X file to add")]
    public string? SourcePath { get; init; }

    [CommandOption("target", 't', Description = "Optional target key override (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    [CommandOption("action", 'a', Description = "Snapshot action: Append, ReplaceLatest, or ReplaceAll")]
    public SnapshotAction Action { get; init; } = SnapshotAction.Append;

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new CommandException("File path is required.", ErrorCodes.UsageError);

        if (!File.Exists(SourcePath))
            throw new CommandException($"File not found: {SourcePath}", ErrorCodes.FileNotFound);

        try
        {
            var result = await console.Ansi()
                .Status()
                .StartAsync("Importing source...", async ctx =>
                {
                    ctx.Status("Loading L5X file...");
                    var content = await L5X.LoadAsync(SourcePath, token);
                    var snapshot = Snapshot.Create(content, TargetKey);
                    ctx.Status("Importing source to database...");
                    await database.AddSnapshot(snapshot, Action, token);
                    return snapshot;
                });

            OutputResult(console, result);
        }
        catch (XmlException e)
        {
            throw new CommandException(
                $"Failed to parse L5X file with error: {e.Message}",
                ErrorCodes.FormatError,
                false, e
            );
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"Database import failed with error: {e.Message}",
                ErrorCodes.InternalError,
                false, e
            );
        }
    }

    /// <summary>
    /// Outputs the details of a snapshot result to the console in a tabular format.
    /// </summary>
    /// <param name="console">The console instance used to write the output.</param>
    /// <param name="result">The snapshot result containing the details to display.</param>
    private static void OutputResult(IConsole console, Snapshot result)
    {
        var table = new Table().Border(TableBorder.Rounded).AddColumn("Property").AddColumn("Value");

        table.AddRow("ID", result.SnapshotId.ToString());
        table.AddRow("Key", result.TargetKey);
        table.AddRow("Type", result.TargetType);
        table.AddRow("Name", result.TargetName);
        table.AddRow("Imported", result.ImportDate.ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("Exported", result.ExportDate.ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("Revision", result.SoftwareRevision ?? "?");
        table.AddRow("User", result.ImportUser);
        table.AddRow("Machine", result.ImportMachine);
        table.AddRow("Hash", result.SourceHash);

        console.Ansi().MarkupLine("[green]✓[/] Snapshot imported successfully");
        console.Ansi().Write(table);
    }
}