using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Data.Abstractions;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents a command for pruning snapshots and other related resources in the LogixDb CLI.
/// </summary>
/// <remarks>
/// The <see cref="PruneCommand"/> class provides functionality to delete specific snapshots,
/// snapshots imported before a specific date, or the latest snapshot tied to a target.
/// Use the provided command options to specify pruning behavior.
/// This command inherits from <see cref="DbCommand"/>, allowing database connection configuration.
/// </remarks>
/// <example>
/// This command supports the following options:
/// - Target: Specifies the target resource to be pruned.
/// - SnapshotId: Deletes a snapshot with a specific ID.
/// - Before: Deletes snapshots imported before a given date.
/// - Latest: Deletes the latest snapshot for a specified target.
/// </example>
[PublicAPI]
[Command("prune", Description = "Delete snapshots by ID, date, or target")]
public class PruneCommand : DbCommand
{
    [CommandOption("target", 't', Description = "Target key to prune (standard format: targettype://targetname)")]
    public string? Target { get; init; }

    [CommandOption("id", Description = "Delete a snapshot with the specified ID.")]
    public int SnapshotId { get; init; }

    [CommandOption("latest", Description = "Delete the latest snapshot for the specified target")]
    public bool Latest { get; init; }

    [CommandOption("before", Description = "Delete snapshots imported before the specified date.")]
    public string? Before { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database, CancellationToken token)
    {
        ValidateOptions();
        if (SnapshotId > 0) await DeleteById(console, database, token);
        if (Latest) await DeleteByLatest(console, database, token);
        if (!string.IsNullOrWhiteSpace(Before)) await DeleteByDate(console, database, token);
    }

    /// <summary>
    /// Deletes a snapshot with the specified ID from the database.
    /// </summary>
    /// <param name="console">The console instance used to output status messages.</param>
    /// <param name="database">The database instance used to interact with stored snapshots.</param>
    /// <param name="token">The cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="CommandException">Thrown when an error occurs during the deletion of the snapshot.</exception>
    private async ValueTask DeleteById(IConsole console, ILogixDb database, CancellationToken token)
    {
        try
        {
            await database.DeleteSnapshot(SnapshotId, token);
            await console.Output.WriteLineAsync($"Deleted snapshot {SnapshotId}");
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"Prune failed with error: {e.Message}",
                ErrorCodes.InternalError,
                false, e
            );
        }
    }

    /// <summary>
    /// Deletes the latest snapshot for the specified target.
    /// </summary>
    /// <param name="console">The console to write output and error messages.</param>
    /// <param name="database">The database instance to execute the delete operation.</param>
    /// /// <param name="token">The cancellation token used to cancel the asynchronous operation.</param>
    /// <exception cref="CommandException">
    /// Thrown if the target is not specified or if an error occurs during the delete operation.
    /// </exception>
    private async ValueTask DeleteByLatest(IConsole console, ILogixDb database, CancellationToken token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Target))
                throw new CommandException("--latest requires --target to be specified", ErrorCodes.UsageError);

            await database.DeleteSnapshotLatest(Target, token);
            await console.Output.WriteLineAsync($"Deleted latest snapshot for {Target}");
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"Prune failed with error: {e.Message}",
                ErrorCodes.InternalError,
                false, e
            );
        }
    }

    /// <summary>
    /// Deletes snapshots imported before a specified date.
    /// </summary>
    /// <param name="console">The console interface used for output messages.</param>
    /// <param name="database">The database interface that provides functionality for managing snapshots.</param>
    /// /// <param name="token">The cancellation token used to cancel the asynchronous operation.</param>
    /// <exception cref="CommandException">
    /// Thrown when an invalid date format is provided or when a deletion operation fails due to an internal error.
    /// </exception>
    private async ValueTask DeleteByDate(IConsole console, ILogixDb database, CancellationToken token)
    {
        try
        {
            if (!DateTime.TryParse(Before, out var beforeDate))
                throw new CommandException($"Invalid date format: {Before}", ErrorCodes.UsageError);

            await database.DeleteSnapshotsBefore(beforeDate, Target, token);
            await console.Output.WriteLineAsync(
                $"Deleted snapshots before {beforeDate:yyyy-MM-dd}" + (Target != null ? $" for {Target}" : "")
            );
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"Prune failed with error: {e.Message}",
                ErrorCodes.InternalError,
                false, e
            );
        }
    }

    /// <summary>
    /// Validates the combination of options provided for the prune command to ensure correctness and resolve conflicts.
    /// </summary>
    /// <exception cref="CommandException">
    /// Thrown when no delete option is specified or when multiple delete options are provided simultaneously.
    /// </exception>
    private void ValidateOptions()
    {
        var optionCount = 0;
        if (SnapshotId > 0) optionCount++;
        if (Latest) optionCount++;
        if (!string.IsNullOrWhiteSpace(Before)) optionCount++;

        switch (optionCount)
        {
            case 0:
                throw new CommandException(
                    "Missing required arguments. Must specify one delete option (--id, --before, --latest).",
                    ErrorCodes.UsageError
                );
            case > 1:
                throw new CommandException(
                    "Cannot specify multiple delete options (--id, --before, --latest). Choose only one.",
                    ErrorCodes.UsageError
                );
        }
    }
}