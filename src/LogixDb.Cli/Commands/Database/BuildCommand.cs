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
[Command("build", Description = "Creates and migrates the database if it does not exist.")]
public class BuildCommand(ILogixDatabaseFactory factory) : DbCommand(factory)
{
    /// <summary>
    /// Gets or sets a value indicating whether to drop and recreate the database if it already exists.
    /// When set to true, the database will be dropped and reinitialized from scratch,
    /// ensuring a clean setup. This option can be useful for resetting the database during development or testing scenarios.
    /// </summary>
    [CommandOption("rebuild", Description = "Drop and recreate the database if it already exists")]
    public bool Rebuild { get; init; }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(IConsole console, ILogixDatabase database)
    {
        await console.Ansi().Status().StartAsync("Building database...", _ => database.Build(Rebuild));
        console.Ansi().MarkupLine("[green]✓[/] Database built successfully");
    }
}