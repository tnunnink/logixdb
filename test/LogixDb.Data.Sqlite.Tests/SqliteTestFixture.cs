using System.Data;
using Dapper;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Sqlite.Tests;

/// <summary>
/// Provides a test fixture for setting up and managing SQLite-based test environments.
/// This class generates a temporary SQLite database, configures required services, and
/// handles cleanup after tests are executed.
/// </summary>
public abstract class SqliteTestFixture
{
    /// <summary>
    /// The file path for the temporary SQLite database used during the test execution.
    /// This path is dynamically generated to ensure uniqueness for each test run.
    /// </summary>
    protected static readonly string TempDb = Path.Combine(Path.GetTempPath(), $"Logix_{Guid.NewGuid():N}.db");

    /// <summary>
    /// Provides a test fixture for setting up and managing SQLite-based test environments.
    /// This class generates a temporary SQLite database, configures required services, and
    /// handles cleanup after tests are executed.
    /// </summary>
    protected SqliteTestFixture()
    {
        Database = new SqliteDb(new SqlConnectionInfo(SqlProvider.Sqlite, TempDb));
    }

    /// <summary>
    /// Represents an instance of the SQLite database used in tests within the SQLite test fixture.
    /// Provides functionality for lifecycle management, migrations, and other database operations
    /// required during the execution of unit tests.
    /// </summary>
    protected ILogixDb Database { get; }

    /// <summary>
    /// Performs cleanup operations after a test within the fixture has run. This method attempts to
    /// delete the temporary SQLite database file to ensure test isolation and minimal resource usage.
    /// Exceptions during cleanup are handled gracefully as best-effort.
    /// </summary>
    [TearDown]
    protected virtual void TearDown()
    {
        try
        {
            if (File.Exists(TempDb))
                File.Delete(TempDb);
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    /// <summary>
    /// Asserts that a record exists in a specified table with a specific value for a given column.
    /// An exception is thrown if no matching record is found.
    /// </summary>
    /// <param name="tableName">The name of the table to query.</param>
    /// <param name="columnName">The name of the column to inspect for the expected value.</param>
    /// <param name="expected">The value to match in the specified column.</param>
    /// <returns>A task that represents the asynchronous assertion operation.</returns>
    protected async Task AssertRecordExists(string tableName, string columnName, object expected)
    {
        using var connection = await Database.Connect();

        var result = await connection.QuerySingleAsync<int>(
            $"SELECT 1 FROM {tableName} WHERE {columnName} = @expected",
            new { expected }
        );

        Assert.That(result, Is.EqualTo(1), $"No record with '{columnName}={expected}' exists in table '{tableName}'");
    }

    /// <summary>
    /// Asserts that the specified table contains the expected number of records.
    /// </summary>
    /// <param name="tableName">The name of the table to verify the record count for.</param>
    /// <param name="count">The expected number of records in the table.</param>
    /// <exception cref="AssertionException">Thrown when the actual record count does not match the specified count.</exception>
    protected async Task AssertRecordCount(string tableName, int count)
    {
        using var connection = await Database.Connect();
        var result = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
        Assert.That(result, Is.EqualTo(count), $"Invalid record count for table '{tableName}'");
    }

    /// <summary>
    /// Verifies that a specified table exists in the database by checking the SQLite system catalog.
    /// Throws an <c>AssertionException</c> if the table is not found.
    /// </summary>
    /// <param name="tableName">The name of the table to verify existence for.</param>
    /// <returns>
    /// A task that performs the table existence check operation asynchronously.
    /// </returns>
    /// <exception cref="AssertionException">
    /// Thrown when the specified table does not exist in the database.
    /// </exception>
    protected async Task AssertTableExists(string tableName)
    {
        using var connection = await Database.Connect();

        var result = await connection.ExecuteScalarAsync<int>(
            """
            SELECT 1
            FROM sqlite_master
            WHERE type = 'table' AND name = @name
            LIMIT 1;
            """,
            new { name = tableName }
        );

        if (result == 0)
            throw new AssertionException($"Table '{tableName}' was not found in the database.");
    }

    /// <summary>
    /// Validates the definition of a specific column in a specified table within the database.
    /// Ensures the column exists and its type matches the expected type.
    /// </summary>
    /// <param name="tableName">The name of the table containing the column.</param>
    /// <param name="columnName">The name of the column to validate.</param>
    /// <param name="columnType">The expected type of the column.</param>
    /// <returns>
    /// A task representing the asynchronous operation. Throws an <c>AssertionException</c> if the column
    /// does not exist or if its data type does not match the expected type.
    /// </returns>
    /// <exception cref="AssertionException">
    /// Thrown when the column does not exist in the table or the column type does not match the expected type.
    /// </exception>
    protected async Task AssertColumnDefinition(string tableName, string columnName, string columnType)
    {
        using var connection = await Database.Connect();
        var records = await connection.QueryAsync($"PRAGMA table_info('{tableName}');");
        var column = records.SingleOrDefault(r => r.name == columnName);

        if (column is null)
            throw new AssertionException($"Column '{columnName}' was not found in table '{tableName}'.");

        if (!string.Equals(column.type, columnType, StringComparison.OrdinalIgnoreCase))
            throw new AssertionException(
                $"Expected column '{columnName}' in table '{tableName}' to have type '{columnType}', but found type '{column.type}'.");
    }

    /// <summary>
    /// Asserts that the table has a primary key composed of the specified columns.
    /// Order matters (SQLite stores PK column order).
    /// </summary>
    protected async Task AssertPrimaryKey(string tableName, string column)
    {
        using var connection = await Database.Connect();
        var records = await connection.QueryAsync($"PRAGMA table_info('{tableName}');");
        var key = records.SingleOrDefault(r => r.pk > 0 && r.name == column);

        if (key is null)
            throw new AssertionException($"Expected PK {column} on table {tableName}.");
    }

    /// <summary>
    /// Asserts that a foreign key constraint exists between the specified columns in two tables.
    /// Validates the relationship by querying the SQLite foreign key list for the source table.
    /// </summary>
    /// <param name="fromTable">The name of the source table containing the foreign key column.</param>
    /// <param name="fromColumn">The name of the foreign key column in the source table.</param>
    /// <param name="toTable">The name of the target table referenced by the foreign key.</param>
    /// <param name="toColumn">The name of the column in the target table that is referenced.</param>
    /// <returns>A task that represents the completion of the assertion operation.</returns>
    /// <exception cref="AssertionException">
    /// Thrown if the expected foreign key relationship does not exist between the specified tables and columns.
    /// </exception>
    protected async Task AssertForeignKey(string fromTable, string fromColumn, string toTable, string toColumn)
    {
        using var connection = await Database.Connect();
        var records = await connection.QueryAsync($"PRAGMA foreign_key_list('{fromTable}');");
        var key = records.SingleOrDefault(r => r.table == toTable && r.from == fromColumn && r.to == toColumn);

        if (key is null)
            throw new AssertionException(
                $"Expected FK {fromTable}({fromColumn}) -> {toTable}({toColumn}), but none was found.");
    }

    /// <summary>
    /// Asserts that an index exists on the specified table with the given columns.
    /// Throws an <c>AssertionException</c> if no matching index is found.
    /// </summary>
    /// <param name="tableName">The name of the table on which to check for the existence of the index.</param>
    /// <param name="columns">A collection of column names that should define the index.</param>
    /// <returns>A task that represents the completion of the assertion operation.</returns>
    /// <exception cref="AssertionException">Thrown if no index matches the specified columns on the table.</exception>
    protected async Task AssertIndex(string tableName, params string[] columns)
    {
        using var connection = await Database.Connect();
        var records = await connection.QueryAsync($"PRAGMA index_list('{tableName}');");
        var indexes = records.Select(r => r.name).Cast<string>().ToArray();

        var hasMatch = await HasMatchingIndex(connection, columns, indexes);

        if (!hasMatch)
            throw new AssertionException(
                $"""
                 Expected index on '{tableName}' for ({string.Join(", ", columns)}), 
                 but none was found. Found unique indexes: {string.Join(", ", indexes)}
                 """
            );
    }

    /// <summary>
    /// Asserts that a UNIQUE index exists on the specified table with the given columns.
    /// Throws an <c>AssertionException</c> if no matching UNIQUE index is found.
    /// </summary>
    /// <param name="tableName">The name of the table on which to check for the existence of the UNIQUE index.</param>
    /// <param name="columns">A collection of column names that should define the UNIQUE index.</param>
    /// <returns>A task that represents the completion of the assertion operation.</returns>
    /// <exception cref="AssertionException">Thrown if no UNIQUE index matches the specified columns on the table.</exception>
    protected async Task AssertUniqueIndex(string tableName, params string[] columns)
    {
        using var connection = await Database.Connect();
        var records = await connection.QueryAsync($"PRAGMA index_list('{tableName}');");
        var indexes = records.Where(r => r.unique == 1).Select(r => r.name).Cast<string>().ToArray();

        var hasMatch = await HasMatchingIndex(connection, columns, indexes);

        if (!hasMatch)
            throw new AssertionException(
                $"""
                 Expected UNIQUE index on '{tableName}' for ({string.Join(", ", columns)}), 
                 but none was found. Found unique indexes: {string.Join(", ", indexes)}
                 """
            );
    }

    /// <summary>
    /// Checks if there is an existing index in the database that matches the specified column order.
    /// </summary>
    /// <param name="connection">The database connection used to query index information.</param>
    /// <param name="columns">The array of column names that defines the desired order and structure of the index.</param>
    /// <param name="indexes">The array of existing index names associated with the table.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is <c>true</c>
    /// if an index matching the specified column order is found; otherwise, <c>false</c>.
    /// </returns>
    private static async Task<bool> HasMatchingIndex(IDbConnection connection, string[] columns, string[] indexes
    )
    {
        foreach (var indexName in indexes)
        {
            var info = await connection.QueryAsync($"PRAGMA index_info('{indexName}');");
            var columnNames = info.OrderBy(x => x.seqno).Select(x => x.name).Cast<string>().ToArray();
            if (columns.SequenceEqual(columnNames)) return true;
        }

        return false;
    }
}