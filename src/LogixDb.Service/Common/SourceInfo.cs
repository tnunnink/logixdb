namespace LogixDb.Service.Common;

/// <summary>
/// Represents the metadata and source details of a file intended for ingestion or processing.
/// </summary>
public sealed record SourceInfo
{
    /// <summary>
    /// Represents the unique identifier associated with the source of the file.
    /// This identifier is used to distinguish and track individual sources within the system.
    /// </summary>
    public required Guid SourceId { get; init; }

    /// <summary>
    /// Specifies the type of file being processed or referenced.
    /// Used to distinguish between supported file formats in the system,
    /// such as L5X and ACD, for processing or validation purposes.
    /// </summary>
    public required FileType FileType { get; init; }

    /// <summary>
    /// Represents the file path associated with the source file.
    /// This property typically contains the full path location where the file is stored
    /// or retrieved for processing or ingestion.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Represents the name of the file associated with the source.
    /// This property typically includes the file's full path or its designated name,
    /// used for identification and processing in the ingestion workflow.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Represents additional data or descriptive key-value pairs associated with the source.
    /// Provides contextual or supplementary details that are not part of the core properties.
    /// </summary>
    public Dictionary<string, string> Metadata { get; } = [];

    /// <summary>
    /// Creates a new <see cref="SourceInfo"/> instance with a generated unique identifier,
    /// file type parsed from the file extension, and a computed file path within the specified drop directory.
    /// Optionally populates the metadata dictionary with provided key-value pairs.
    /// </summary>
    /// <param name="fileName">The original name of the file, including its extension, used to determine the file type.</param>
    /// <param name="dropPath">The directory path where the file will be stored, combined with the generated source ID and file type.</param>
    /// <param name="metadata">An optional collection of key-value pairs to populate the <see cref="Metadata"/> dictionary.</param>
    /// <returns>A new <see cref="SourceInfo"/> instance with all required properties initialized.</returns>
    public static SourceInfo Create(string fileName, string dropPath, IDictionary<string, string>? metadata = null)
    {
        var sourceId = Guid.NewGuid();
        var fileType = Enum.Parse<FileType>(Path.GetExtension(fileName).Trim('.'));
        var filePath = Path.Combine(dropPath, $"{sourceId:N}.{fileType}");

        var source = new SourceInfo
        {
            SourceId = sourceId,
            FileType = fileType,
            FilePath = filePath,
            FileName = fileName
        };

        if (metadata is not null)
        {
            foreach (var kvp in metadata)
                source.Metadata[kvp.Key] = kvp.Value;
        }

        return source;
    }
}