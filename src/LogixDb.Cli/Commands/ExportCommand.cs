using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Data.Abstractions;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents a command for exporting snapshots to L5X files in the LogixDb CLI.
/// </summary>
/// <remarks>
/// The <see cref="ExportCommand"/> class provides functionality to export a specific snapshot
/// to an L5X file by specifying either a target key (to export the latest snapshot for that target)
/// or a snapshot ID (to export a specific snapshot).
/// This command inherits from <see cref="DbCommand"/>, allowing database connection configuration.
/// </remarks>
/// <example>
/// This command supports the following options:
/// - Target: Exports the latest snapshot for the specified target key.
/// - SnapshotId: Exports a snapshot with a specific ID.
/// - OutputPath: Specifies the output file path for the exported L5X file.
/// </example>
[PublicAPI]
[Command("export", Description = "Exports a snapshot to an L5X file by target or ID")]
public class ExportCommand : DbCommand
{
    [CommandOption("output", 'o', Description = "Output file path for the exported L5X file (defaults to <TargetKey>.L5X)")]
    public string? OutputPath { get; init; }

    [CommandOption("target", 't', Description = "Target key to export (exports the latest snapshot for this target)")]
    public string? TargetKey { get; init; }

    [CommandOption("id", Description = "Export a snapshot with the specified ID")]
    public int SnapshotId { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database, CancellationToken token)
    {
        ValidateOptions();

        try
        {
            var snapshot = !string.IsNullOrWhiteSpace(TargetKey)
                ? await database.GetSnapshotLatest(TargetKey, token)
                : await database.GetSnapshotById(SnapshotId, token);

            var savePath = OutputPath ?? $"{snapshot.TargetKey}.L5X";
            var source = snapshot.GetSource();
            source.Save(savePath);
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"Export failed with error: {e.Message}",
                ErrorCodes.InternalError,
                false, e
            );
        }
    }

    /// <summary>
    /// Validates the options provided for the command execution to ensure correct usage.
    /// This method checks whether the required options (--target or --id) are specified,
    /// and ensures only one of these options is provided at a time.
    /// Throws a <see cref="CommandException"/> if the validation fails.
    /// </summary>
    /// <exception cref="CommandException">
    /// Thrown when none of the required options (--target, --id) are provided,
    /// or when multiple options are specified.
    /// The error code <see cref="ErrorCodes.UsageError"/> is used to indicate the validation error.
    /// </exception>
    private void ValidateOptions()
    {
        var optionCount = 0;
        if (!string.IsNullOrWhiteSpace(TargetKey)) optionCount++;
        if (SnapshotId > 0) optionCount++;

        switch (optionCount)
        {
            case 0:
                throw new CommandException(
                    "Missing required arguments. Must specify one option (--target, --id).",
                    ErrorCodes.UsageError
                );
            case > 1:
                throw new CommandException(
                    "Cannot specify multiple export options (--target, --id).",
                    ErrorCodes.UsageError
                );
        }
    }
}