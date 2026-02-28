namespace LogixDb.Service.Common;

/// <summary>
/// Represents metadata for an asset, including its unique identifier, version information, and name.
/// </summary>
public class AssetInfo
{
    /// <summary>
    /// Gets the unique identifier for the asset.
    /// </summary>
    public Guid AssetId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the specific version of the asset.
    /// </summary>
    public Guid VersionId { get; init; }

    /// <summary>
    /// Gets the display name of the asset.
    /// </summary>
    public string AssetName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the version number of the asset.
    /// </summary>
    public int VersionNumber { get; init; }
}