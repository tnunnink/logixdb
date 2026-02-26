using L5Sharp.Core;

namespace LogixDb.Data;

/// <summary>
/// Represents a snapshot of an L5X file containing Logix controller data.
/// This class captures metadata about the export, including target information, schema details,
/// and maintains both a hash and the compressed XML source data for versioning and retrieval purposes.
/// Snapshots are persisted to the database and linked to a target through the snapshot table.
/// </summary>
public sealed class Snapshot
{
    /// <summary>
    /// A private field representing the parsed L5X data associated with the snapshot.
    /// This field is lazily initialized when the source data is decompressed and parsed,
    /// and it serves as the in-memory representation of the XML data from the L5X file.
    /// </summary>
    private L5X? _l5X;

    public int SnapshotId { get; set; }
    public string TargetKey { get; init; } = string.Empty;
    public string TargetType { get; init; } = string.Empty;
    public string TargetName { get; init; } = string.Empty;
    public bool IsPartial { get; init; }
    public string? SchemaRevision { get; init; }
    public string? SoftwareRevision { get; init; }
    public DateTime ExportDate { get; init; } = DateTime.MinValue;
    public string? ExportOptions { get; init; }
    public DateTime ImportDate { get; init; } = DateTime.UtcNow;
    public string ImportUser { get; init; } = Environment.UserName;
    public string ImportMachine { get; init; } = Environment.MachineName;
    public byte[] SourceHash { get; init; } = [];
    public byte[] SourceData { get; init; } = [];

    /// <summary>
    /// Creates a new snapshot instance from an L5X source file.
    /// Extracts metadata from the L5X content and generates a hash of the serialized source data.
    /// </summary>
    /// <param name="source">The L5X source file containing Logix controller data to snapshot.</param>
    /// <param name="targetKey">An optional custom target key. If not provided, a default key is generated from the
    /// target type and name in the format "targettype://targetname".</param>
    /// <returns>A new Snapshot instance populated with metadata and source data from the L5X file.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the L5X content does not contain a valid TargetType or TargetName.
    /// </exception>
    public static Snapshot Create(L5X source, string? targetKey = null)
    {
        if (source.Content.TargetType is null)
            throw new InvalidOperationException(
                "The L5X content does not contain a valid TargetType. Cannot create snapshot without a target type.");

        if (source.Content.TargetName is null)
            throw new InvalidOperationException(
                "The L5X content does not contain a valid TargetName. Cannot create snapshot without a target name.");

        return new Snapshot
        {
            TargetKey = targetKey ?? $"{source.Content.TargetType.ToLower()}://{source.Content.TargetName}",
            TargetType = source.Content.TargetType,
            TargetName = source.Content.TargetName,
            IsPartial = source.Content.ContainsContext,
            SchemaRevision = source.Content.SchemaRevision,
            SoftwareRevision = source.Content.SoftwareRevision,
            ExportDate = source.Content.ExportDate,
            ExportOptions = string.Join(",", source.Content.ExportOptions),
            SourceHash = source.Content.Serialize().ToString().Hash(),
            SourceData = source.Content.Serialize().ToString().Compress(),
            _l5X = source
        };
    }

    /// <summary>
    /// Retrieves the parsed L5X source data associated with the snapshot.
    /// If the parsed data has not yet been initialized, it is created by decompressing and parsing
    /// the stored source data.
    /// </summary>
    /// <returns>The parsed L5X instance representing the source data of the snapshot.</returns>
    public L5X GetSource() => _l5X ??= L5X.Parse(SourceData.Decompress());

    /// <summary>
    /// Returns a string representation of the snapshot instance.
    /// The representation includes the target type and target name in a standardized format.
    /// </summary>
    /// <returns>A string in the format "targettype://targetname".</returns>
    public override string ToString() => TargetKey;
}