using LogixDb.Data;

namespace LogixDb.Service.Configuration;

/// <summary>
/// Represents the configuration settings for the ingestion process in the Logix system.
/// </summary>
public class IngestionConfig
{
    /// <summary>
    /// The file path where uploaded files are dropped to be processed by the background worker service. 
    /// </summary>
    public string DropPath { get; set; } = string.Empty;

    /// <summary>
    /// The connection string to the database that is the target for ingestion.
    /// </summary>
    public string DbConnection { get; set; } = string.Empty;

    /// <summary>
    /// Defines the action to be taken when importing a new snapshot into the database.
    /// This determines how the current snapshots will be handled in relation to the new snapshot being imported.
    /// </summary>
    public SnapshotAction OnImport { get; set; } = SnapshotAction.ReplaceLatest;
}