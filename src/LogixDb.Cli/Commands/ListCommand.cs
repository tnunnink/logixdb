using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Data;
using LogixDb.Data.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents a command for listing all snapshots in the database, optionally filtered by a target key.
/// </summary>
/// <remarks>
/// This command retrieves and displays snapshots from the database. The results can be filtered
/// by specifying a target key, which is expected in the format "targettype://targetname".
/// The output includes a table of snapshot details such as ID, target key, target type,
/// target name, revision, user, machine, and import/export dates.
/// </remarks>
/// <example>
/// Use this command to query and review snapshots stored in the database.
/// The filtering by target key can help narrow results for specific targets.
/// </example>
[PublicAPI]
[Command("list", Description = "Lists all snapshots, optionally filtered by target key")]
public class ListCommand : DbCommand
{
    [CommandOption("target", 't', Description = "Optional target key filter (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database, CancellationToken token)
    {
        try
        {
            var snapshots = await console.Ansi()
                .Status()
                .StartAsync("Retrieving snapshots...", _ => database.ListSnapshots(TargetKey, token));

            OutputSnapshots(console, snapshots.ToList());
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"List snapshots failed with error: {e.Message}",
                ErrorCodes.InternalError,
                false, e
            );
        }
    }

    /// <summary>
    /// Displays a formatted table of snapshots to the console.
    /// </summary>
    /// <param name="console">The console instance used for rendering output.</param>
    /// <param name="snapshots">The list of snapshots to display.</param>
    private static void OutputSnapshots(IConsole console, List<Snapshot> snapshots)
    {
        if (snapshots.Count == 0)
        {
            console.Ansi().MarkupLine("[yellow]No snapshots found[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded)
            .AddColumn("Id")
            .AddColumn("Target Key")
            .AddColumn("Target Type")
            .AddColumn("Target Name")
            .AddColumn("Revision")
            .AddColumn("Exported")
            .AddColumn("Imported")
            .AddColumn("User")
            .AddColumn("Machine")
            .AddColumn("Hash");

        foreach (var snapshot in snapshots.OrderByDescending(s => s.ImportDate))
        {
            table.AddRow(
                snapshot.SnapshotId.ToString(),
                snapshot.TargetKey,
                snapshot.TargetType,
                snapshot.TargetName,
                snapshot.SoftwareRevision ?? "N/A",
                snapshot.ExportDate.ToString("yyyy-MM-dd HH:mm:ss"),
                snapshot.ImportDate.ToString("yyyy-MM-dd HH:mm:ss"),
                snapshot.ImportUser,
                snapshot.ImportMachine,
                snapshot.SourceHash.ToHexString()
            );
        }

        console.Ansi().Write(table);
        console.Ansi().MarkupLine($"\n[green]Total:[/] {snapshots.Count} snapshot(s)");
    }
}