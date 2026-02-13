using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands.Snapshots;

/// <summary>
/// Represents a command to export the most recent snapshot for a target from the database.
/// </summary>
[PublicAPI]
[Command("snapshot export", Description = "Exports the most recent snapshot for a target to an L5X file")]
public class SnapshotExportCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    [CommandParameter(1, Name = "target", Description = "Target key (format: targettype://targetname)")]
    public string TargetKey { get; init; } = string.Empty;

    [CommandOption("output", 'o', Description = "Output file path (defaults to <targetname>.L5X)")]
    public string? OutputPath { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        if (string.IsNullOrWhiteSpace(TargetKey))
            throw new CommandException("Target key is required.", ExitCodes.UsageError);

        var snapshot = await AnsiConsole.Status()
            .StartAsync("Exporting snapshot...", async ctx =>
            {
                ctx.Status("Retrieving snapshot from database...");
                return await database.Export(TargetKey);
            });

        var outputPath = OutputPath ?? $"{snapshot.TargetName}.L5X";

        console.Ansi().Status().Start($"Writing to {outputPath}...", _ => { snapshot.GetSource().Save(outputPath); });
        console.Ansi().MarkupLine($"[green]✓[/] Snapshot exported successfully to [blue]{outputPath}[/]");
    }
}