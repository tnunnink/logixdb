namespace LogixDb.Service.Common;

public sealed record AssetInfo
{
    public required Guid AssetId { get; init; }

    public required Guid VersionId { get; init; }

    public required string AssetName { get; init; }
    
    public required string AssetSource { get; init; }
}