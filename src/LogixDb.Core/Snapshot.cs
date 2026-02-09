using L5Sharp.Core;

namespace LogixDb.Core;

/// <summary>
/// Represents a snapshot of an L5X file containing Logix controller data.
/// This class captures metadata about the export, including target information, schema details,
/// and maintains both a hash and the compressed XML source data for versioning and retrieval purposes.
/// Snapshots are persisted to the database and linked to a target through the snapshot table.
/// </summary>
public sealed class Snapshot
{
    public int SnapshotId { get; init; }
    public string TargetType { get; init; } = string.Empty;
    public string TargetName { get; init; } = string.Empty;
    public bool IsPartial { get; init; }
    public string? SchemaRevision { get; init; }
    public string? SoftwareRevision { get; init; }
    public DateTime ExportDate { get; init; } = DateTime.MinValue;
    public string? ExportOptions { get; init; }
    public DateTime ImportDate { get; init; } = DateTime.UtcNow;
    public string? ImportUser { get; init; } = Environment.UserDomainName;
    public string ImportMachine { get; init; } = Environment.MachineName;
    public string SourceHash { get; init; } = string.Empty;
    public byte[] SourceData { get; init; } = [];

    /// <summary>
    /// Creates a new snapshot instance from an L5X source file.
    /// Extracts metadata from the L5X content and generates a hash of the serialized source data.
    /// </summary>
    /// <param name="source">The L5X source file containing Logix controller data to snapshot.</param>
    /// <returns>A new Snapshot instance populated with metadata and source data from the L5X file.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the L5X content does not contain a valid TargetType or TargetName.
    /// </exception>
    public static Snapshot Create(L5X source)
    {
        if (source.Content.TargetType is null)
            throw new InvalidOperationException(
                "The L5X content does not contain a valid TargetType. Cannot create snapshot without a target type.");

        if (source.Content.TargetName is null)
            throw new InvalidOperationException(
                "The L5X content does not contain a valid TargetName. Cannot create snapshot without a target name.");

        return new Snapshot
        {
            TargetType = source.Content.TargetType,
            TargetName = source.Content.TargetName,
            IsPartial = source.Content.ContainsContext,
            SchemaRevision = source.Content.SchemaRevision,
            SoftwareRevision = source.Content.SoftwareRevision,
            ExportDate = source.Content.ExportDate,
            ExportOptions = string.Join(",", source.Content.ExportOptions),
            SourceHash = source.Content.Serialize().ToString().Hash(),
            SourceData = source.Content.Serialize().ToString().Compress()
        };
    }

    /// <summary>
    /// Generates the default key for this snapshot in the format "targettype://targetname".
    /// This key is used to uniquely identify the target in the database and for lookup operations.
    /// </summary>
    /// <returns>A string key in the format "targettype://targetname" with the target type in lowercase.</returns>
    public string GetDefaultKey() => $"{TargetType.ToLower()}://{TargetName}";

    /// <summary>
    /// Retrieves the decompressed XML source data for the snapshot.
    /// The data is uncompressed from the stored binary format and returned as a string.
    /// </summary>
    /// <returns>The decompressed XML source data as a string.</returns>
    public string GetSource() => SourceData.Decompress();

    /// <summary>
    /// Returns a string representation of the snapshot instance.
    /// The representation includes the target type and target name in a standardized format.
    /// </summary>
    /// <returns>A string in the format "targettype://targetname".</returns>
    public override string ToString() => $"{TargetType.ToLower()}://{TargetName}";
}