using CliFx.Attributes;
using CliFx.Infrastructure;
using JetBrains.Annotations;
using LogixDb.Cli.Common;
using LogixDb.Core.Abstractions;
using Spectre.Console;

namespace LogixDb.Cli.Commands.Database;

/// <summary>
/// Represents a command to build/create the database if it does not exist.
/// This command initializes the database structure without using migrations.
/// </summary>
[PublicAPI]
[Command("build", Description = "Creates the database if it does not exist (without migrations)")]
public class BuildCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    /// <inheritdoc />
    protected override ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        console.Ansi().Status().Start("Building database...", _ => { database.Build(); });
        console.Ansi().MarkupLine("[green]✓[/] Database built successfully");
        return ValueTask.CompletedTask;
    }
}