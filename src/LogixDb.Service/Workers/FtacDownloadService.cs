using System.Data;
using System.Threading.Channels;
using LogixDb.Service.Common;
using LogixDb.Service.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LogixDb.Service.Workers;

/// <summary>
/// Provides background processing functionality for downloading assets and synchronizing
/// them with source information. This service reads asset details from a channel, processes
/// them by simulating a download or retrieval operation, and writes the resulting source
/// information back to another channel for downstream operations or storage.
/// </summary>
public class FtacDownloadService(
    Channel<AssetInfo> assets,
    Channel<SourceInfo> sources,
    IOptions<LogixConfig> options,
    ILogger<FtacDownloadService> logger
) : BackgroundService
{
    /// <summary>
    /// Specifies the size of each data chunk, in bytes, for processing operations.
    /// This value is used to determine the maximum amount of data to be read or written
    /// in a single iteration during asset download or other chunked operations.
    /// </summary>
    /// <remarks>
    /// 1MB is a good chunk size as it speeds up the download significatly.
    /// </remarks>
    private const int ChunkSize = 1048576;

    /// <summary>
    /// Represents the connection string used to establish a connection to the SQL Server database.
    /// The connection string is built using parameters such as server name, database name,
    /// and connection settings provided by the application configuration.
    /// </summary>
    /// <remarks>
    /// This field uses integrated security and trusts the server certificate by default.
    /// It is dynamically constructed based on the configuration settings from <see cref="LogixConfig"/>.
    /// </remarks>
    private readonly string _connectionString = new SqlConnectionStringBuilder
    {
        DataSource = options.Value.FtacService.Server,
        InitialCatalog = options.Value.FtacService.Database,
        IntegratedSecurity = true,
        TrustServerCertificate = true
    }.ConnectionString;

    /// <summary>
    /// Executes the background task to process assets asynchronously. It reads assets from
    /// the provided channel, downloads them, and writes the resulting source information to another channel.
    /// </summary>
    /// <param name="token">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>A task that represents the asynchronous execution of the background operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await foreach (var asset in assets.Reader.ReadAllAsync(token))
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Downloading asset {FileName}...", asset.AssetName);

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(token);

                var size = await ReadAssetSize(connection, asset, token);
                var source = await DownloadAsset(connection, asset, size, token);

                await sources.Writer.WriteAsync(source, token);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Queued {FileName} for processing", source.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error downloading asset {FileName}", asset.AssetName);
            }
        }
    }

    /// <summary>
    /// Reads the size of an asset by executing a stored procedure on the provided database connection.
    /// The procedure retrieves details about the file size and version, ensuring the asset is valid for processing.
    /// </summary>
    /// <param name="connection">The SQL connection to the database where the asset information is stored.</param>
    /// <param name="asset">An object containing the unique identifiers of the asset and its version.</param>
    /// <param name="token">A cancellation token to monitor for operation cancellation.</param>
    /// <returns>The size of the asset in bytes.</returns>
    /// <exception cref="Exception">Thrown when the stored procedure returns a non-zero result code.</exception>
    private async Task<long> ReadAssetSize(SqlConnection connection, AssetInfo asset, CancellationToken token)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Reading asset info for {AssetName} - v{VersionNumber}",
                asset.AssetName, asset.VersionNumber);

        await using var command = new SqlCommand("dbo.arch_ReadFileChunkStart", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@AssetId", SqlDbType.UniqueIdentifier).Value = asset.AssetId;
        command.Parameters.Add("@VersionId", SqlDbType.UniqueIdentifier).Value = asset.VersionId;
        command.Parameters.Add("@VersionNumber", SqlDbType.Int).Direction = ParameterDirection.Output;

        // This is what needs to be used to pass the correct length when reading file chunks.
        var fileLength = command.Parameters.Add("@FileLength", SqlDbType.BigInt);
        fileLength.Direction = ParameterDirection.Output;

        // This can just be used to throw when we fail to call the procedure correctly.
        var result = command.Parameters.Add("@Result", SqlDbType.Int);
        result.Direction = ParameterDirection.Output;

        // These parameters are required by the procedure signature even if not used here
        command.Parameters.Add("@ChecksumCRC32", SqlDbType.BigInt).Direction = ParameterDirection.Output;
        command.Parameters.Add("@ChecksumSHA1", SqlDbType.NVarChar, 40).Direction = ParameterDirection.Output;
        command.Parameters.Add("@ChecksumSHA256", SqlDbType.NVarChar, 64).Direction = ParameterDirection.Output;
        command.Parameters.Add("@InUse", SqlDbType.TinyInt).Direction = ParameterDirection.Output;
        command.Parameters.Add("@InUseUncPath", SqlDbType.NVarChar, 1024).Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync(token);

        if ((int)result.Value != 0)
            throw new Exception($"arch_ReadFileChunkStart failed with ResultCode: {result.Value}");

        return (long)fileLength.Value;
    }

    /// <summary>
    /// Downloads the specified asset from the database and saves it to a local file.
    /// The method reads asset chunks from the database using a stored procedure
    /// and writes them incrementally to the file system while reporting progress.
    /// </summary>
    /// <param name="connection">An open SQL connection used to retrieve the asset data.</param>
    /// <param name="asset">The metadata of the asset to be downloaded, including identifiers and name.</param>
    /// <param name="length">The total length of the asset in bytes, used for determining chunk sizes and progress.</param>
    /// <param name="token">A cancellation token to observe while performing the asynchronous operation.</param>
    /// <returns>A task that produces a <see cref="SourceInfo"/> object containing information about the downloaded asset upon completion.</returns>
    /// <exception cref="SqlException">Thrown if any SQL operation encounters an error during execution.</exception>
    /// <exception cref="IOException">Thrown if the file operation fails while writing the downloaded asset to the file system.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the provided cancellation token.</exception>
    private async Task<SourceInfo> DownloadAsset(
        SqlConnection connection,
        AssetInfo asset,
        long length,
        CancellationToken token)
    {
        // Ensure the drop path exists in case no uploads have ever run.
        var dropPath = options.Value.IngestionService.DropPath;
        Directory.CreateDirectory(dropPath);

        var source = SourceInfo.Create(asset.AssetName, dropPath);

        await using var stream = new FileStream(source.FilePath, FileMode.Create, FileAccess.Write, FileShare.None);

        await using var command = new SqlCommand("dbo.arch_ReadFileChunk", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@AssetId", SqlDbType.UniqueIdentifier).Value = asset.AssetId;
        command.Parameters.Add("@VersionId", SqlDbType.UniqueIdentifier).Value = asset.VersionId;
        var offsetParameter = command.Parameters.Add("@Offset", SqlDbType.Int);
        var offsetSize = command.Parameters.Add("@Size", SqlDbType.Int);

        long offset = 0;
        while (offset < length)
        {
            var size = (int)Math.Min(ChunkSize, length - offset);
            offsetParameter.Value = (int)offset;
            offsetSize.Value = size;

            await using var reader = await command.ExecuteReaderAsync(token);

            if (await reader.ReadAsync(token))
            {
                var bytes = (byte[])reader.GetValue(0);
                await stream.WriteAsync(bytes, token);
            }
            else
            {
                throw new Exception($"No data returned for offset={offset} size={size}");
            }

            offset += size;
            var percent = (double)offset / length * 100;

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("\rProgress: {Percent:F2}% ({Offset}/{FileLength} bytes)",
                    percent, offset, length);
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Successfully downloaded asset {FileName}", asset.AssetName);

        return source;
    }
}