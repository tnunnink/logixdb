using System.Data;
using FluentMigrator.Runner;
using LogixDb.Migrations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Sqlite.Tests;

public abstract class SqliteMigrationTestBase
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    protected SqliteMigrationTestBase(string? customPath = null)
    {
        _dbPath = customPath ?? Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        _connectionString = $"Data Source={_dbPath};";
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    protected void MigrateUp(long? toVersion = null)
    {
        var services = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(_connectionString)
                .WithVersionTable(new MigrationTableMetaData())
                .ScanIn(
                    typeof(MigrationTableMetaData).Assembly,
                    typeof(SqliteRegistration).Assembly
                ).For.Migrations())
            .BuildServiceProvider(validateScopes: false);

        using var scope = services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        if (toVersion is null)
            runner.MigrateUp();
        else
            runner.MigrateUp(toVersion.Value);
    }

    protected IDbConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    /// <summary>
    /// Asserts that the specified table exists in the database.
    /// </summary>
    /// <param name="connection">The database connection to use for the assertion.</param>
    /// <param name="tableName">The name of the table to check for existence.</param>
    /// <exception cref="AssertionException">Thrown if the table does not exist in the database.</exception>
    protected static void AssertTableExists(IDbConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();

        command.CommandText =
            """
            SELECT 1
            FROM sqlite_master
            WHERE type = 'table' AND name = @name
            LIMIT 1;
            """;

        var nameParameter = command.CreateParameter();
        nameParameter.ParameterName = "@name";
        nameParameter.Value = tableName;
        command.Parameters.Add(nameParameter);


        if (command.ExecuteScalar() is null)
            throw new AssertionException($"Table '{tableName}' was not found in the database.");
    }

    /// <summary>
    /// Asserts that the specified column exists in the table with the expected data type.
    /// </summary>
    /// <param name="connection">The database connection to use for the assertion.</param>
    /// <param name="tableName">The name of the table containing the column.</param>
    /// <param name="columnName">The name of the column to check.</param>
    /// <param name="columnType">The expected data type of the column.</param>
    /// <exception cref="AssertionException">
    /// Thrown if the column does not exist in the specified table or if the column's data type does not match the expected type.
    /// </exception>
    protected static void AssertColumnDefinition(IDbConnection connection, string tableName, string columnName,
        string columnType)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName}');";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var name = reader.GetString(reader.GetOrdinal("name"));

            if (!string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
                continue;

            var type = reader.GetString(reader.GetOrdinal("type"));

            if (!string.Equals(type, columnType, StringComparison.OrdinalIgnoreCase))
                throw new AssertionException(
                    $"Expected column '{columnName}' in table '{tableName}' to have type '{columnType}', but found type '{type}'.");

            return;
        }

        throw new AssertionException(
            $"Column '{columnName}' was not found in table '{tableName}'.");
    }

    /// <summary>
    /// Asserts that the table has a primary key composed of the specified columns.
    /// Order matters (SQLite stores PK column order).
    /// </summary>
    protected static void AssertPrimaryKey(IDbConnection conn, string tableName, params string[] columns)
    {
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("At least one PK column must be provided.", nameof(columns));

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info('{tableName}');";

        var primaryKeys = new List<(int PkOrder, string Name)>();

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                // pk is 0 if not part of PK, otherwise 1..N for composite PK order
                var pk = reader.GetInt32(reader.GetOrdinal("pk"));
                if (pk <= 0) continue;

                var name = reader.GetString(reader.GetOrdinal("name"));
                primaryKeys.Add((pk, name));
            }
        }

        var ordered = primaryKeys
            .OrderBy(x => x.PkOrder)
            .Select(x => x.Name)
            .ToArray();

        if (ordered.Length != columns.Length)
            throw new AssertionException(
                $"Expected PK({string.Join(", ", columns)}) on '{tableName}', but found PK({string.Join(", ", ordered)}).");

        for (var i = 0; i < columns.Length; i++)
        {
            if (!string.Equals(columns[i], ordered[i], StringComparison.OrdinalIgnoreCase))
                throw new AssertionException(
                    $"Expected PK column {i + 1} to be '{columns[i]}' on '{tableName}', but found '{ordered[i]}'. " +
                    $"Actual PK: ({string.Join(", ", ordered)}).");
        }
    }

    /// <summary>
    /// Asserts there exists a UNIQUE index on the specified columns (in the specified order).
    /// Note: SQLite enforces UNIQUE via either a unique index or a unique constraint (also exposed as an index).
    /// </summary>
    protected static void AssertUniqueIndex(IDbConnection conn, string tableName, params string[] columns)
    {
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("At least one column must be provided.", nameof(columns));

        var table = tableName.Replace("'", "''");
        var wanted = columns.Select(c => c.Trim()).ToArray();

        using var listCmd = conn.CreateCommand();
        listCmd.CommandText = $"PRAGMA index_list('{table}');";

        var uniqueIndexes = new List<string>();

        using (var reader = listCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                // columns: seq, name, unique, origin, partial
                var isUnique = reader.GetInt32(reader.GetOrdinal("unique")) == 1;
                if (!isUnique) continue;

                var indexName = reader.GetString(reader.GetOrdinal("name"));
                uniqueIndexes.Add(indexName);
            }
        }

        foreach (var indexName in uniqueIndexes)
        {
            var indexCols = GetIndexColumns(conn, indexName);
            if (indexCols.Length != wanted.Length) continue;

            var match = true;
            for (var i = 0; i < wanted.Length; i++)
            {
                if (!string.Equals(wanted[i], indexCols[i], StringComparison.OrdinalIgnoreCase))
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return;
        }

        throw new AssertionException(
            $"Expected UNIQUE index on '{tableName}' for ({string.Join(", ", columns)}), " +
            $"but none was found. Found unique indexes: {string.Join(", ", uniqueIndexes)}");
    }

    /// <summary>
    /// Asserts that a foreign key exists from table(fromColumn) to toTable(toColumn).
    /// </summary>
    protected static void AssertForeignKey(
        IDbConnection conn,
        string fromTable,
        string fromColumn,
        string toTable,
        string toColumn)
    {
        var from = fromTable.Replace("'", "''");

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA foreign_key_list('{from}');";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            // columns: id, seq, table, from, to, on_update, on_delete, match
            var fkToTable = reader.GetString(reader.GetOrdinal("table"));
            var fkFromCol = reader.GetString(reader.GetOrdinal("from"));
            var fkToCol = reader.GetString(reader.GetOrdinal("to"));

            if (string.Equals(fkToTable, toTable, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(fkFromCol, fromColumn, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(fkToCol, toColumn, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        throw new AssertionException(
            $"Expected FK {fromTable}({fromColumn}) -> {toTable}({toColumn}), but none was found.");
    }

    private static string[] GetIndexColumns(IDbConnection conn, string indexName)
    {
        using var command = conn.CreateCommand();
        command.CommandText = $"PRAGMA index_info('{indexName}');";

        var columns = new List<(int SeqNo, string Name)>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var seqNo = reader.GetInt32(reader.GetOrdinal("seqno"));
            var name = reader.GetString(reader.GetOrdinal("name"));
            columns.Add((seqNo, name));
        }

        return columns.OrderBy(x => x.SeqNo).Select(x => x.Name).ToArray();
    }
    
    /// <summary>
    /// Asserts there exists a (non-unique) index on the specified columns (in the specified order).
    /// </summary>
    protected static void AssertIndex(IDbConnection connection, string tableName, params string[] columns)
    {
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("At least one column must be provided.", nameof(columns));

        var table = tableName.Replace("'", "''");
        var wanted = columns.Select(c => c.Trim()).ToArray();

        using var listCmd = connection.CreateCommand();
        listCmd.CommandText = $"PRAGMA index_list('{table}');";

        var allIndexes = new List<string>();

        using (var reader = listCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var indexName = reader.GetString(reader.GetOrdinal("name"));
                allIndexes.Add(indexName);
            }
        }

        foreach (var indexName in allIndexes)
        {
            var indexCols = GetIndexColumns(connection, indexName);
            if (indexCols.Length != wanted.Length) continue;

            var match = true;
            for (var i = 0; i < wanted.Length; i++)
            {
                if (string.Equals(wanted[i], indexCols[i], StringComparison.OrdinalIgnoreCase)) continue;
                match = false;
                break;
            }

            if (match)
                return;
        }

        throw new AssertionException(
            $"Expected index on '{tableName}' for ({string.Join(", ", columns)}), " +
            $"but none was found. Found indexes: {string.Join(", ", allIndexes)}");
    }
}