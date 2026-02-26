namespace LogixDb.Service.Configuration;

/// <summary>
/// Represents the configuration settings for the Logix system.
/// </summary>
public class LogixConfig
{
    /// <summary>
    /// Defines the ingestion configuration settings, including options for how data is imported into the system.
    /// </summary>
    public IngestionConfig IngestionService { get; init; } = new();

    /// <summary>
    /// Represents the configuration options for monitoring FTAC (FactoryTalk AssetCentre) system connections,
    /// including server address, database name, port, and optional data filters.
    /// </summary>
    public FtacConfig FtacService { get; init; } = new();
}