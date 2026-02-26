namespace LogixDb.Data;

/// <summary>
/// Specifies the options available for handling database snapshots when adding a new snapshot.
/// </summary>
public enum SnapshotAction
{
    /// <summary>
    /// Indicates that the new snapshot should be appended to the existing list of snapshots.
    /// This option preserves all previously added snapshots and adds the new snapshot as an additional entry.
    /// </summary>
    Append,

    /// <summary>
    /// Indicates that the latest snapshot should be replaced with the new snapshot.
    /// This option discards the most recently added snapshot while retaining all others in the list.
    /// </summary>
    ReplaceLatest,

    /// <summary>
    /// Indicates that all existing snapshots should be replaced with the new snapshot.
    /// This option removes all previously stored snapshots before adding the new one,
    /// ensuring that only the latest snapshot is retained in the database.
    /// </summary>
    ReplaceAll
}