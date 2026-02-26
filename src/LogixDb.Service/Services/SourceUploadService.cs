using System.Threading.Channels;
using LogixDb.Service.Common;
using LogixDb.Service.Configuration;
using Microsoft.Extensions.Options;

namespace LogixDb.Service.Services;

/// <summary>
/// Handles the uploading and queuing of source files for further processing.
/// </summary>
/// <remarks>
/// This class is responsible for managing the transfer of files from an incoming stream
/// to a designated drop path, ensuring that a unique file name is generated to prevent conflicts.
/// It also queues the uploaded files for further processing using a provided channel.
/// </remarks>
/// <param name="channel">
/// The channel used to queue the source information for downstream processing.
/// </param>
/// <param name="options">
/// The configuration options containing the drop path for uploaded files.
/// </param>
/// <param name="logger">
/// The logger instance used to log information, errors, and warnings during the upload process.
/// </param>
public class SourceUploadService(Channel<SourceInfo> channel, IOptions<LogixConfig> options, ILogger<SourceUploadService> logger)
{
    /// <summary>
    /// Asynchronously uploads a file to the server and queues it for processing.
    /// </summary>
    /// <param name="file">The file to upload, represented as an <see cref="IFormFile"/>.</param>
    /// <param name="metadata">A collection of metadata associated with the file. Can be null.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an instance of <see cref="SourceInfo"/>
    /// with information about the uploaded file.
    /// </returns>
    public async Task<SourceInfo> UploadAsync(IFormFile file, IDictionary<string, string> metadata)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Upload requested for file: {FileName} ({FileSize} bytes)",
                file.FileName, file.Length);

        // Ensure the drop path is always available
        var dropPath = options.Value.IngestionService.DropPath;
        Directory.CreateDirectory(dropPath);

        // Create the source record from the provided args
        var source = SourceInfo.Create(file, dropPath, metadata);

        // Upload the file to the local server drop path
        await using (var stream = new FileStream(source.FilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("File successfully uploaded to: {Path}", source.FilePath);

        // Queue the source for processing by the background service
        await channel.Writer.WriteAsync(source);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Queued {FileName} for processing", source.FileName);

        return source;
    }
}