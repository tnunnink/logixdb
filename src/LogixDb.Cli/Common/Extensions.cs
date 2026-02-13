using CliFx;
using CliFx.Infrastructure;
using LogixDb.Cli.Services;
using LogixDb.Core.Abstractions;
using LogixDb.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace LogixDb.Cli.Common;

/// <summary>
/// Provides extension methods for enhancing the functionality of existing types
/// within the LogixDb.Cli.Common namespace.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Creates an instance of <see cref="IAnsiConsole"/> using the specified console
    /// output, applying settings for ANSI support and color system detection.
    /// </summary>
    /// <param name="console">
    /// The console to use for output. This represents the underlying console interface
    /// where ANSI-rendered output will be written.
    /// </param>
    /// <returns>An instance of <see cref="IAnsiConsole"/> configured with the specified settings.</returns>
    public static IAnsiConsole Ansi(this IConsole console)
    {
        return AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            Out = new AnsiConsoleOutput(console.Output)
        });
    }

    /// <summary>
    /// Adds LogixDb services and configurations to the provided CliApplicationBuilder instance.
    /// </summary>
    /// <param name="builder">
    /// The CliApplicationBuilder instance to which the LogixDb services will be added.
    /// </param>
    /// <returns>
    /// The updated CliApplicationBuilder instance configured with LogixDb services.
    /// </returns>
    public static CliApplicationBuilder AddLogixDb(this CliApplicationBuilder builder)
    {
        return builder.UseTypeActivator(commands =>
        {
            var services = new ServiceCollection();
            services.AddTransient<ILogixDatabaseFactory, DatabaseFactory>();
            services.AddLogixSqlite();
            //services.AddLogixSqlServer();

            foreach (var commandType in commands)
                services.AddTransient(commandType);

            return services.BuildServiceProvider();
        });
    }
}