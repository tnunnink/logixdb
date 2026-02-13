using System.Data;

namespace LogixDb.SqlServer.Tests;

public abstract class SqlServerMigrationTestBase
{
    /// <summary>
    /// Asserts that the specified table exists in the database.
    /// </summary>
    protected static void AssertTableExists(IDbConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();

        command.CommandText =
            """
            SELECT 1
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = @tableName
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        if (command.ExecuteScalar() is null)
            throw new AssertionException($"Table '{tableName}' was not found in the database.");
    }

    /// <summary>
    /// Asserts that the specified column exists in the table with the expected data type.
    /// </summary>
    protected static void AssertColumnDefinition(IDbConnection connection, string tableName, string columnName,
        string columnType)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT DATA_TYPE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName
            """;

        var tableParam = command.CreateParameter();
        tableParam.ParameterName = "@tableName";
        tableParam.Value = tableName;
        command.Parameters.Add(tableParam);

        var columnParam = command.CreateParameter();
        columnParam.ParameterName = "@columnName";
        columnParam.Value = columnName;
        command.Parameters.Add(columnParam);

        var result = command.ExecuteScalar() as string;

        if (result is null)
            throw new AssertionException($"Column '{columnName}' was not found in table '{tableName}'.");

        if (!string.Equals(result, columnType, StringComparison.OrdinalIgnoreCase))
            throw new AssertionException(
                $"Expected column '{columnName}' in table '{tableName}' to have type '{columnType}', but found type '{result}'.");
    }

    /// <summary>
    /// Asserts that the table has a primary key composed of the specified columns.
    /// </summary>
    protected static void AssertPrimaryKey(IDbConnection connection, string tableName, params string[] columns)
    {
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("At least one PK column must be provided.", nameof(columns));

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
            AND TABLE_NAME = @tableName
            ORDER BY ORDINAL_POSITION";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var pkColumns = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            pkColumns.Add(reader.GetString(0));
        }

        if (pkColumns.Count != columns.Length)
            throw new AssertionException(
                $"Expected PK({string.Join(", ", columns)}) on '{tableName}', but found PK({string.Join(", ", pkColumns)}).");

        for (var i = 0; i < columns.Length; i++)
        {
            if (!string.Equals(columns[i], pkColumns[i], StringComparison.OrdinalIgnoreCase))
                throw new AssertionException(
                    $"Expected PK column {i + 1} to be '{columns[i]}' on '{tableName}', but found '{pkColumns[i]}'. " +
                    $"Actual PK: ({string.Join(", ", pkColumns)}).");
        }
    }

    /// <summary>
    /// Asserts that a foreign key exists from table(fromColumn) to toTable(toColumn).
    /// </summary>
    protected static void AssertForeignKey(
        IDbConnection connection,
        string fromTable,
        string fromColumn,
        string toTable,
        string toColumn)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 1
            FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu1 
                ON rc.CONSTRAINT_NAME = kcu1.CONSTRAINT_NAME
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu2 
                ON rc.UNIQUE_CONSTRAINT_NAME = kcu2.CONSTRAINT_NAME
            WHERE kcu1.TABLE_NAME = @fromTable 
                AND kcu1.COLUMN_NAME = @fromColumn
                AND kcu2.TABLE_NAME = @toTable 
                AND kcu2.COLUMN_NAME = @toColumn";

        var fromTableParam = command.CreateParameter();
        fromTableParam.ParameterName = "@fromTable";
        fromTableParam.Value = fromTable;
        command.Parameters.Add(fromTableParam);

        var fromColumnParam = command.CreateParameter();
        fromColumnParam.ParameterName = "@fromColumn";
        fromColumnParam.Value = fromColumn;
        command.Parameters.Add(fromColumnParam);

        var toTableParam = command.CreateParameter();
        toTableParam.ParameterName = "@toTable";
        toTableParam.Value = toTable;
        command.Parameters.Add(toTableParam);

        var toColumnParam = command.CreateParameter();
        toColumnParam.ParameterName = "@toColumn";
        toColumnParam.Value = toColumn;
        command.Parameters.Add(toColumnParam);

        if (command.ExecuteScalar() is null)
            throw new AssertionException(
                $"Expected FK {fromTable}({fromColumn}) -> {toTable}({toColumn}), but none was found.");
    }
}