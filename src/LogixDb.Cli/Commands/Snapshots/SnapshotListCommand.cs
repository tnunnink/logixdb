using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands.Snapshots;

/// <summary>
/// Represents a command to list all snapshots in the database, optionally filtered by target key.
/// </summary>
[PublicAPI]
[Command("snapshot list", Description = "Lists all snapshots, optionally filtered by target key")]
public class SnapshotListCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    [CommandOption("target", 't', Description = "Optional target key filter (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        var snapshots = await console.Ansi()
            .Status()
            .StartAsync("Retrieving snapshots...", async _ => await database.ListSnapshots(TargetKey));

        var snapshotList = snapshots.ToList();

        if (snapshotList.Count == 0)
        {
            console.Ansi().MarkupLine("[yellow]No snapshots found[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("Target Key")
            .AddColumn("Target Type")
            .AddColumn("Target Name")
            .AddColumn("Import Date")
            .AddColumn("Software Rev");

        foreach (var snapshot in snapshotList.OrderByDescending(s => s.ImportDate))
        {
            table.AddRow(
                snapshot.SnapshotId.ToString(),
                snapshot.TargetKey,
                snapshot.TargetType,
                snapshot.TargetName,
                snapshot.ImportDate.ToString("yyyy-MM-dd HH:mm:ss"),
                snapshot.SoftwareRevision ?? "N/A"
            );
        }

        console.Ansi().Write(table);
        console.Ansi().MarkupLine($"\n[green]Total:[/] {snapshotList.Count} snapshot(s)");
    }
}