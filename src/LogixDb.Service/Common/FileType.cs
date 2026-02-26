namespace LogixDb.Service.Common;

/// <summary>
/// Specifies the supported Rockwell Automation Logix file types for ingestion and processing.
/// </summary>
public enum FileType
{
    /// <summary>
    /// Represents a Logix 5000 XML export file format (.L5X), which contains controller configuration and logic in XML format.
    /// </summary>
    L5X,
    
    /// <summary>
    /// Represents a Logix Designer archive/project file format (.ACD), which contains the complete controller project in binary format.
    /// </summary>
    ACD
}