using Dapper;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using Testcontainers.MsSql;

namespace LogixDb.SqlServer.Tests;

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
    /// Represents a test container instance for managing a Microsoft SQL Server database lifecycle
    /// within the SqlServerTestFixture. This container facilitates integration testing by providing
    /// an isolated and disposable SQL Server environment.
    /// </summary>
    /// <remarks>
    /// The container is implemented using the Testcontainers library and provides mechanisms
    /// to start, stop, and dispose of the SQL Server instance. It is configured with the
    /// proper SQL Server image and settings for use with the fixture.
    /// </remarks>
    /// <value>
    /// An instance of <see cref="MsSqlContainer"/> used to manage the lifecycle of the SQL Server
    /// container for testing purposes.
    /// </value>
    private MsSqlContainer _container;

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
    protected ILogixDb Database { get; private set; }

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
    /// Performs a one-time setup for initializing the SQL Server test environment. This includes
    /// creating and starting a SQL Server container using Testcontainers, configuring the
    /// connection details, and initializing a database instance to be used in integration tests.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeSetUp]
    protected async Task OneTimeSetup()
    {
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
            .WithPassword("LogixDb!Test123")
            .Build();

        await _container.StartAsync();

        var connection = new SqlConnectionInfo(
            SqlProvider.SqlServer,
            _container.Hostname,
            Port: _container.GetMappedPublicPort(),
            User: "sa",
            Password: "LogixDb!Test123"
        );

        Database = new SqlServerDb(connection);
    }

    /// <summary>
    /// Performs a one-time teardown for cleaning up the SQL Server test environment. This includes
    /// disposing of the database instance and stopping and disposing of the SQL Server container.
    /// This method ensures that all resources used in the integration tests are properly released.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeTearDown]
    protected async Task OneTimeTearDown()
    {
        await Database.DisposeAsync();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Verifies the existence of a specific table in the database by querying the
    /// INFORMATION_SCHEMA.TABLES view. If the table does not exist, an exception is thrown.
    /// </summary>
    /// <param name="tableName">The name of the table to check for existence in the database.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="AssertionException">Thrown if the specified table does not exist in the database.</exception>
    protected async Task AssertTableExists(string tableName)
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
    /// Verifies the existence and data type of specified column in a given table within the database.
    /// Throws an assertion exception if the column does not exist or its data type does not match the expected type.
    /// </summary>
    /// <param name="tableName">The name of the table in which the column is located.</param>
    /// <param name="columnName">The name of the column to verify.</param>
    /// <param name="columnType">The expected data type of the column.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task AssertColumnDefinition(string tableName, string columnName, string columnType)
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
    protected async Task AssertPrimaryKey(string tableName, string columnName)
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
    protected async Task AssertForeignKey(string fromTable, string fromColumn, string toTable, string toColumn)
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
}