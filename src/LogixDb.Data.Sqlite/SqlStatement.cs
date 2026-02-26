namespace LogixDb.Data.Sqlite;

/// <summary>
/// Provides a collection of SQL query strings used for interacting with a SQLite database
/// in the "snapshot" and related tables.
/// </summary>
internal static class SqlStatement
{
    /// <summary>
    /// A SQL query string used to retrieve a list of snapshot entries from the database table "snapshot."
    /// The query supports filtering snapshots based on the associated target key by matching it with the
    /// target_id retrieved from the "target" table. If no target key is specified, all snapshots are returned.
    /// </summary>
    internal const string ListSnapshots =
        """
        SELECT snapshot_id [SnapshotId],
              t.target_id [TargetId],
              t.target_key [TargetKey],
              target_type [TargetType],
              target_name [TargetName],
              is_partial [IsPartial],
              schema_revision [SchemaRevision],
              software_revision [SoftwareRevision],
              export_date [ExportDate],
              export_options [ExportOptions],
              import_date [ImportDate],
              import_user [ImportUser],
              import_machine [ImportMachine],
              source_hash [SourceHash] 
        FROM snapshot s
        JOIN target t on t.target_id = s.target_id
        WHERE @target_key is null or t.target_key = @target_key
        """;

    /// <summary>
    /// A SQL query string used to fetch a snapshot entry from the database table "snapshot"
    /// where the snapshot ID matches the specified parameter.
    /// </summary>
    internal const string GetSnapshotById =
        """
        SELECT snapshot_id [SnapshotId],
              t.target_id [TargetId],
              t.target_key [TargetKey],
              target_type [TargetType],
              target_name [TargetName],
              is_partial [IsPartial],
              schema_revision [SchemaRevision],
              software_revision [SoftwareRevision],
              export_date [ExportDate],
              export_options [ExportOptions],
              import_date [ImportDate],
              import_user [ImportUser],
              import_machine [ImportMachine],
              source_hash [SourceHash], 
              source_data [SourceData] 
        FROM snapshot s
        JOIN target t on t.target_id = s.target_id
        WHERE snapshot_id = @snapshot_id
        """;

    /// <summary>
    /// A SQL query string used to retrieve the most recent snapshot entry from the database table "snapshot"
    /// based on the target ID associated with the specified target key.
    /// </summary>
    internal const string GetLatestSnapshot =
        """
        SELECT snapshot_id [SnapshotId],
              t.target_id [TargetId],
              t.target_key [TargetKey],
              target_type [TargetType],
              target_name [TargetName],
              is_partial [IsPartial],
              schema_revision [SchemaRevision],
              software_revision [SoftwareRevision],
              export_date [ExportDate],
              export_options [ExportOptions],
              import_date [ImportDate],
              import_user [ImportUser],
              import_machine [ImportMachine],
              source_hash [SourceHash], 
              source_data [SourceData] 
        FROM snapshot s
        JOIN target t on t.target_id = s.target_id
        WHERE t.target_key = @target_key
        ORDER BY import_date DESC
        LIMIT 1
        """;

    /// <summary>
    /// A SQL query string used to delete all entries from the "target" database table.
    /// This query ensures that every record in the table where the target_id is greater than zero is removed,
    /// effectively clearing the table of all targets.
    /// </summary>
    internal const string DeleteAllTargets =
        "DELETE FROM target WHERE target_id > 0";

    /// <summary>
    /// A SQL query string used to delete a target entry from the "target" table in the database.
    /// The deletion is performed by matching the target_key value with the key provided via the @target_key parameter.
    /// </summary>
    internal const string DeleteTargetById = 
        "DELETE FROM target where target_key = @target_key ";

    /// <summary>
    /// A SQL query string designed to delete a specific snapshot entry from the database
    /// table "snapshot" based on the provided snapshot ID. This query ensures the removal
    /// of the snapshot record identified by the "snapshot_id" parameter.
    /// </summary>
    internal const string DeleteSnapshotById = 
        "DELETE FROM snapshot WHERE snapshot_id = @snapshot_id;";

    /// <summary>
    /// A SQL query string used to delete the latest snapshot entry from the "snapshot" table, determined by the most recent
    /// import_date. The query optionally filters snapshots by a specified target key, matching it to the target_id from
    /// the "target" table. If no target key is provided, the latest snapshot is deleted regardless of its associated target.
    /// </summary>
    internal const string DeleteSnapshotByLatest =
        """
        DELETE FROM snapshot 
        WHERE snapshot_id = (
            SELECT snapshot_id 
            FROM snapshot 
            WHERE (@target_key IS NULL OR target_id = (SELECT target_id FROM target WHERE target_key = @target_key))
            ORDER BY import_date DESC 
            LIMIT 1
        )
        """;

    /// <summary>
    /// A SQL query string used to delete snapshot records from the "snapshot" table that were imported
    /// before a specified date. If a target key is provided, the deletion is further scoped to snapshots
    /// associated with the specified target key by matching it to the target_id in the "target" table.
    /// If no target key is provided, the query deletes all matching snapshots solely based on the import date.
    /// </summary>
    internal const string DeleteSnapshotsBefore =
        """
        DELETE FROM snapshot 
               WHERE (@target_key is null or target_id = (SELECT target_id FROM target where target_key = @target_key))
               AND import_date < @import_date
        """;
}