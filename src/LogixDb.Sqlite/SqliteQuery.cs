namespace LogixDb.Sqlite;

/// <summary>
/// Provides a collection of SQL query strings used for interacting with a SQLite database
/// in the "snapshot" and related tables.
/// </summary>
public static class SqliteQuery
{
    /// <summary>
    /// A SQL query string used to retrieve a list of snapshot entries from the database table "snapshot."
    /// The query supports filtering snapshots based on the associated target key by matching it with the
    /// target_id retrieved from the "target" table. If no target key is specified, all snapshots are returned.
    /// </summary>
    public const string ListSnapshots =
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
              source_hash [ImportHash] 
        FROM snapshot s
        JOIN target t on t.target_id = s.target_id
        WHERE @target_key is null or t.target_key = @target_key
        """;

    /// <summary>
    /// A SQL query string used to fetch a snapshot entry from the database table "snapshot"
    /// where the snapshot ID matches the specified parameter.
    /// </summary>
    public const string GetSnapshotById =
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
    public const string GetLatestSnapshot =
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
}