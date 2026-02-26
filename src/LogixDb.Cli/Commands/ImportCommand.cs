using System.Xml;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using L5Sharp.Core;
using LogixConverter.LogixSdk;
using LogixDb.Cli.Common;
using LogixDb.Data;
using LogixDb.Data.Abstractions;
using Spectre.Console;
using Snapshot = LogixDb.Data.Snapshot;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents a command to import an L5X file as a new snapshot into the database.
/// </summary>
[PublicAPI]
[Command("import", Description = "Imports an L5X file as a new snapshot into the database")]
public class ImportCommand : DbCommand
{
    [CommandOption("source", 's', IsRequired = true, Description = "Path to the source L5X file to add")]
    public string? SourcePath { get; init; }

    [CommandOption("target", 't', Description = "Optional target key override (format: targettype://targetname)")]
    public string? TargetKey { get; init; }

    [CommandOption("action", 'a', Description = "The action to take when importing the snapshot.")]
    public SnapshotAction Action { get; init; } = SnapshotAction.Append;

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDb database, CancellationToken token)
    {
        if (!File.Exists(SourcePath))
            throw new CommandException($"File not found: {SourcePath}", ErrorCodes.FileNotFound);

        if (StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(SourcePath), ".acd"))
        {
            var tempSource = await ConvertFileAsync(console, Path.GetFullPath(SourcePath), token);
            await ImportFileAsync(console, database, tempSource, token);
            File.Delete(tempSource);
            return;
        }

        await ImportFileAsync(console, database, Path.GetFullPath(SourcePath), token);
    }

    /// <summary>
    /// Imports a specified L5X file into the database as a new snapshot.
    /// </summary>
    /// <param name="console">The console interface used to display output to the user.</param>
    /// <param name="database">The database instance where the file will be imported.</param>
    /// <param name="importTarget">The fully qualified path to the target L5X file to import.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous import operation.</returns>
    /// <exception cref="CommandException">
    /// Thrown when the L5X file fails to parse, or the import operation encounters an error.
    /// </exception>
    private async ValueTask ImportFileAsync(IConsole console, ILogixDb database, string importTarget,
        CancellationToken token)
    {
        try
        {
            var result = await console.Ansi().Status().StartAsync("Importing source...", async ctx =>
            {
                ctx.Status("Loading L5X file...");
                var content = await L5X.LoadAsync(importTarget, token);
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
    /// Converts an ACD file to a specified format and returns the path of the converted file.
    /// </summary>
    /// <param name="console">The console interface used for displaying progress and messages.</param>
    /// <param name="sourcePath">The path to the source ACD file to be converted.</param>
    /// <param name="token">A token for observing cancellation requests.</param>
    /// <returns>The path to the converted file.</returns>
    private static async ValueTask<string> ConvertFileAsync(IConsole console, string sourcePath,
        CancellationToken token)
    {
        //todo ideally would like to inject but allow app to specify implementation
        var converter = new LogixSdkConverter();

        var destination = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}{Path.GetFileNameWithoutExtension(sourcePath)}.L5X"
        );

        try
        {
            var result = await console.Ansi().Status().StartAsync("Converting ACD file...",
                _ => converter.ConvertAsync(sourcePath, destination, token: token)
            );

            if (!result.Success)
            {
                throw new CommandException(
                    $"Project conversion failed with error: {result.Error}",
                    ErrorCodes.SystemError
                );
            }

            return destination;
        }
        catch (CommandException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new CommandException(
                $"Project conversion failed with error: {e.Message}",
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
        table.AddRow("Revision", result.SoftwareRevision ?? "?");
        table.AddRow("Exported", result.ExportDate.ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("Imported", result.ImportDate.ToString("yyyy-MM-dd HH:mm:ss"));
        table.AddRow("User", result.ImportUser);
        table.AddRow("Machine", result.ImportMachine);
        table.AddRow("Hash", result.SourceHash.ToHexString());

        console.Ansi().MarkupLine("[green]✓[/] Snapshot imported successfully");
        console.Ansi().Write(table);
    }
}