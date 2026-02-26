using System.Threading.Channels;
using L5Sharp.Core;
using LogixConverter.Abstractions;
using LogixDb.Data;
using LogixDb.Data.Abstractions;
using LogixDb.Data.Exceptions;
using LogixDb.Service.Common;
using LogixDb.Service.Configuration;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;

namespace LogixDb.Service.Services;

/// <summary>
/// A background service that ingests sources from a channel and processes them using the specified database
/// and file conversion services. This service ensures that ingestion tasks are handled asynchronously, logging
/// progress and errors as needed.
/// </summary>
/// <remarks>
/// The <see cref="SourceIngestionService"/> listens to an unbounded channel for incoming <see cref="SourceInfo"/> items
/// to process. Each item represents a file to be ingested, and the service coordinates database updates,
/// file conversion, and logging for each task. This class inherits from <see cref="BackgroundService"/>,
/// ensuring lifecycle management is integrated with the ASP.NET Core service hosting environment.
/// </remarks>
/// <param name="channel">
/// The asynchronous channel from which <see cref="SourceInfo"/> items are read for processing. This serves as
/// the primary mechanism to manage ingestion task flow.
/// </param>
/// <param name="logixDb">
/// The database abstraction used to perform associated operations such as migrations or data updates during
/// ingestion tasks.
/// </param>
/// <param name="fileConverter">
/// The file converter used for transforming source files into a standardized format. This ensures the integrity
/// of data before persisting it into the database.
/// </param>
/// <param name="options">
/// Configuration options related to the service, injected via <see cref="IOptions{T}"/> to allow for flexible,
/// runtime-configurable settings.
/// </param>
/// <param name="logger">
/// The logger instance for capturing informational messages, warnings, and errors throughout the lifecycle
/// of the service. Useful for monitoring and diagnosing the service in production environments.
/// </param>
public class SourceIngestionService(
    Channel<SourceInfo> channel,
    ILogixDb logixDb,
    ILogixFileConverter fileConverter,
    IHostApplicationLifetime lifetime,
    IOptions<LogixConfig> options,
    ILogger<SourceIngestionService> logger) : BackgroundService
{
    /// <summary>
    /// Executes the asynchronous background processing operation for ingesting sources from a channel reader.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that can be used to signal the operation should stop.</param>
    /// <returns>A task that represents the lifecycle of the background execution process.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Startup Validation (Pre-flight check): If the database is not created or migrated, shutdown the app.
        if (await IsInvalidConnection(stoppingToken))
        {
            lifetime.StopApplication();
            return;
        }

        await foreach (var source in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Processing {FileName}...", source.FileName);

                // Create a temp L5X file for processing, either converting it from ACD or just copying it, depending on the file type.
                var tempFile = await ConvertOrCopy(source, stoppingToken);

                // Load the L5X file, create a snapshot and add it to the database.
                var content = await L5X.LoadAsync(tempFile, stoppingToken);
                var snapshot = Snapshot.Create(content);
                var action = options.Value.IngestionService.OnImport;
                await logixDb.AddSnapshot(snapshot, action, stoppingToken);

                // Clean up temp and upload files after processing completes.
                File.Delete(tempFile);
                File.Delete(source.FilePath);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Successfully processed {FileName}", source.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ingestion task for {FileName}", source.FileName);
            }
        }
    }

    /// <summary>
    /// Determines whether the current database connection is invalid by attempting to verify connectivity.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to observe while performing the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a boolean value indicating whether the connection is invalid.</returns>
    private async Task<bool> IsInvalidConnection(CancellationToken stoppingToken)
    {
        try
        {
            await logixDb.ListSnapshots(token: stoppingToken);
            logger.LogInformation("Database connection verified. Ingestion service started and waiting uploads...");
        }
        catch (MigrationRequiredException ex)
        {
            logger.LogCritical(ex,
                "Database migration is required. Manual migration must be performed before the service can start.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Failed to connect to the LogixDb target. Ensure the connection string is correct and the database is reachable.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts or copies a source file to a temporary .L5X file, depending on its type.
    /// </summary>
    /// <param name="source">The source information containing the file metadata and type.</param>
    /// <param name="token">The cancellation token to observe while performing the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the path to the temporary .L5X file.</returns>
    private async Task<string> ConvertOrCopy(SourceInfo source, CancellationToken token)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{source.SourceId:N}.L5X");

        switch (source.FileType)
        {
            case FileType.L5X:
                await CopyToTempFile(source, tempFile, token);
                break;
            case FileType.ACD:
                await ConvertToTempFile(source, tempFile, token);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source.FileType, "Unsupported file type.");
        }

        return tempFile;
    }

    /// <summary>
    /// Converts the content of the source file to the L5X format and writes it to the specified temporary file location.
    /// </summary>
    /// <param name="source">The source information containing the file metadata and location.</param>
    /// <param name="tempFile">The path to the temporary file where the converted content will be written.</param>
    /// <param name="token">The cancellation token to observe while performing the operation.</param>
    /// <returns>A task that represents the asynchronous conversion operation.</returns>
    private async Task ConvertToTempFile(SourceInfo source, string tempFile, CancellationToken token)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Converting {FileName} to temp L5X for processing", source.FileName);

        var result = await fileConverter.ConvertAsync(source.FilePath, tempFile, token: token);

        if (!result.Success)
        {
            //throw new custom exception? 
        }
    }

    /// <summary>
    /// Copies the content of the source file to a temporary file at the specified path.
    /// </summary>
    /// <param name="source">The source information containing the file metadata and location.</param>
    /// <param name="tempFile">The path to the temporary file where the content will be copied.</param>
    /// <param name="token">The cancellation token to observe while performing the operation.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    private async Task CopyToTempFile(SourceInfo source, string tempFile, CancellationToken token)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Copying {FileName} to temp file for processing", source.FileName);

        await using var reader = File.OpenRead(source.FilePath);
        await using var writer = File.Create(tempFile);
        await reader.CopyToAsync(writer, token);
    }
}