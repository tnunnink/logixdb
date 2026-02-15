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
public class SnapshotDeleteCommand : DbCommand
{
    [CommandOption("target", 't', Description = "Target key to purge (format: targettype://targetname)")]
    public string? Target { get; init; }

    [CommandOption("force", 'f', Description = "Skip confirmation prompt")]
    public bool Force { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database)
    {
        throw new NotImplementedException();
    }
}