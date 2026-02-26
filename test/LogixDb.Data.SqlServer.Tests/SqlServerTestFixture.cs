using System.Data;
using Dapper;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.SqlServer.Tests;

/// <summary>
/// Represents a base test fixture for managing a SQL Server container and
/// a corresponding database instance for integration tests. This class
/// sets up and tears down the necessary resources for running tests
/// against a SQL Server database, using Testcontainers.MsSql for
/// container management.
/// </summary>
public abstract class SqlServerTestFixture
{
    /// <summary>
    /// Represents an instance of the database used for testing within the SqlServerTestFixture.
    /// Provides functionality to manage the database lifecycle, including setup, teardown,
    /// and integration with the test container for SQL Server.
    /// </summary>
    /// <remarks>
    /// The property is instantiated as a concrete implementation of <see cref="ILogixDb"/>
    /// (specifically <see cref="SqlServerDb"/>) using a generated SQL Server test container.
    /// </remarks>
    /// <value>
    /// An instance of <see cref="ILogixDb"/> used to interact with the database.
    /// </value>
    protected static ILogixDb Database => SqlServerTestContainer.Database;

    /// <summary>
    /// Cleans up after each test by dropping the database instance used during the test.
    /// This ensures the database is reset and no residual data persists between tests,
    /// maintaining isolation and consistency across test cases.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TearDown]
    protected virtual async Task TearDown()
    {
        await Database.Drop();
    }

    /// <summary>
    /// Verifies the existence of a specific table in the database by querying the
    /// INFORMATION_SCHEMA.TABLES view. If the table does not exist, an exception is thrown.
    /// </summary>
    /// <param name="tableName">The name of the table to check for existence in the database.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="AssertionException">Thrown if the specified table does not exist in the database.</exception>
    protected static async Task AssertTableExists(string tableName)
    {
        using var connection = await Database.Connect();

        var result = await connection.QuerySingleAsync<int>(
            """
            SELECT 1
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = @tableName
            """,
            new { tableName }
        );

        if (result < 1)
            throw new AssertionException($"Table '{tableName}' was not found in the database.");
    }

    /// <summary>
    /// Verifies the existence and data type for the specified column in a given table within the database.
    /// Throws an assertion exception if the column does not exist or its data type does not match the expected type.
    /// </summary>
    /// <param name="tableName">The name of the table in which the column is located.</param>
    /// <param name="columnName">The name of the column to verify.</param>
    /// <param name="columnType">The expected data type of the column.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected static async Task AssertColumnDefinition(string tableName, string columnName, string columnType)
    {
        using var connection = await Database.Connect();

        var result = await connection.QuerySingleAsync<string>(
            """
            SELECT DATA_TYPE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName
            """,
            new { tableName, columnName }
        );

        if (result is null)
            throw new AssertionException($"Column '{columnName}' was not found in table '{tableName}'.");

        if (!string.Equals(result, columnType, StringComparison.OrdinalIgnoreCase))
            throw new AssertionException(
                $"Expected column '{columnName}' in table '{tableName}' to have type '{columnType}', but found type '{result}'.");
    }

    /// <summary>
    /// Verifies that the specified column in the given table is defined as a primary key.
    /// Throws an AssertionException if the primary key constraint is not found.
    /// </summary>
    /// <param name="tableName">The name of the table to check for the primary key.</param>
    /// <param name="columnName">The name of the column expected to be the primary key.</param>
    /// <returns>A task representing the asynchronous validation operation.</returns>
    protected static async Task AssertPrimaryKey(string tableName, string columnName)
    {
        using var connection = await Database.Connect();

        var result = await connection.QuerySingleAsync<int>(
            """
            SELECT 1 
            FROM 
                INFORMATION_SCHEMA.TABLE_CONSTRAINTS t, 
                INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE c 
            WHERE 
                c.Constraint_Name = t.Constraint_Name
                AND c.Table_Name = t.Table_Name
                AND t.Constraint_Type = 'PRIMARY KEY'
                AND c.Table_Name = @tableName
                AND c.COLUMN_NAME = @columnName
            """,
            new { tableName, columnName }
        );

        if (result < 1)
            throw new AssertionException(
                $"Expected primary key on column '{columnName}' in table '{tableName}', but none was found.");
    }

    /// <summary>
    /// Verifies that a foreign key constraint exists between the specified columns and tables in the database.
    /// </summary>
    /// <param name="fromTable">The name of the table containing the foreign key column.</param>
    /// <param name="fromColumn">The name of the column in the source table that forms the foreign key.</param>
    /// <param name="toTable">The name of the table containing the referenced primary key column.</param>
    /// <param name="toColumn">The name of the column in the target table that is referenced by the foreign key.</param>
    /// <returns>A task representing the asynchronous operation, which throws an <see cref="AssertionException"/> if the foreign key does not exist.</returns>
    protected static async Task AssertForeignKey(string fromTable, string fromColumn, string toTable, string toColumn)
    {
        using var connection = await Database.Connect();

        var result = await connection.QuerySingleAsync<int>(
            """
            SELECT 1
            FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk 
                ON rc.CONSTRAINT_NAME = fk.CONSTRAINT_NAME
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk
                ON rc.UNIQUE_CONSTRAINT_NAME = pk.CONSTRAINT_NAME
                AND pk.ORDINAL_POSITION = fk.ORDINAL_POSITION
            WHERE fk.TABLE_NAME = @fromTable 
              AND fk.COLUMN_NAME = @fromColumn 
              AND pk.TABLE_NAME = @toTable
              AND pk.COLUMN_NAME = @toColumn;
            """,
            new { fromTable, fromColumn, toTable, toColumn }
        );

        if (result < 1)
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
    protected static async Task AssertIndex(string tableName, params string[] columns)
    {
        using var connection = await Database.Connect();

        var indexes = (await connection.QueryAsync<string>(
            """
            SELECT i.name
            FROM sys.indexes i
            INNER JOIN sys.tables t ON i.object_id = t.object_id
            WHERE t.name = @tableName
              AND i.is_primary_key = 0
              AND i.type > 0
            """,
            new { tableName }
        )).ToArray();

        var hasMatch = await HasMatchingIndex(connection, tableName, columns, indexes);

        if (!hasMatch)
            throw new AssertionException(
                $"""
                 Expected index on '{tableName}' for ({string.Join(", ", columns)}),
                 but none was found. Found indexes: {string.Join(", ", indexes)}
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
    protected static async Task AssertUniqueIndex(string tableName, params string[] columns)
    {
        using var connection = await Database.Connect();

        var indexes = (await connection.QueryAsync<string>(
            """
            SELECT i.name
            FROM sys.indexes i
            INNER JOIN sys.tables t ON i.object_id = t.object_id
            WHERE t.name = @tableName
              AND i.is_unique = 1
              AND i.is_primary_key = 0
              AND i.type > 0
            """,
            new { tableName }
        )).ToArray();

        var hasMatch = await HasMatchingIndex(connection, tableName, columns, indexes);

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
    /// <param name="tableName">The name of the table containing the indexes.</param>
    /// <param name="columns">The array of column names that defines the desired order and structure of the index.</param>
    /// <param name="indexes">The array of existing index names associated with the table.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is <c>true</c>
    /// if an index matching the specified column order is found; otherwise, <c>false</c>.
    /// </returns>
    private static async Task<bool> HasMatchingIndex(IDbConnection connection,
        string tableName,
        string[] columns,
        string[] indexes
    )
    {
        foreach (var indexName in indexes)
        {
            var columnNames = await connection.QueryAsync<string>(
                """
                SELECT c.name
                FROM sys.index_columns ic
                INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                INNER JOIN sys.tables t ON i.object_id = t.object_id
                WHERE t.name = @tableName
                  AND i.name = @indexName
                ORDER BY ic.key_ordinal
                """,
                new { tableName, indexName }
            );

            if (columns.SequenceEqual(columnNames)) return true;
        }

        return false;
    }
}