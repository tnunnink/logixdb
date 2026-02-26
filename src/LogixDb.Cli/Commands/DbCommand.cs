using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Data;
using LogixDb.Data.Abstractions;

namespace LogixDb.Cli.Commands;

/// <summary>
/// Represents the base class for CLI commands that interact with a database.
/// This class handles common database-related operations such as configuring
/// connection information and executing commands asynchronously.
/// </summary>
[PublicAPI]
public abstract class DbCommand : ICommand
{
    [CommandOption("connection", 'c', IsRequired = true, Description =
        "The database connection path. For SQLite, specify a file path. For SQL Server, use format 'database@host'.")]
    public string Connection { get; init; } = string.Empty;

    /// <summary>
    /// Executes the asynchronous command logic.
    /// Prepares the necessary SQL connection information based on the input parameters
    /// and delegates execution to an implementation-specific method.
    /// </summary>
    /// <param name="console">The console through which the command interacts with the user.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    public ValueTask ExecuteAsync(IConsole console)
    {
        if (string.IsNullOrWhiteSpace(Connection))
            throw new CommandException("Database argument 'connection' is required.", ErrorCodes.UsageError);

        var connection = SqlConnectionInfo.Parse(Connection);
        var database = DbProvider.GetDatabase(connection);
        var cancellation = console.RegisterCancellationHandler();
        return ExecuteAsync(console, database, cancellation);
    }

    /// <summary>
    /// Executes the command-specific logic using the provided console and database connection.
    /// This method must be implemented by derived classes to define the actual command behavior.
    /// </summary>
    /// <param name="console">The console interface for interacting with the user and displaying output.</param>
    /// <param name="database">The connected Logix database instance to operate on.</param>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    protected abstract ValueTask ExecuteAsync(IConsole console, ILogixDb database, CancellationToken token);
}