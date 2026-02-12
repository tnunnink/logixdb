using CliFx;
using CliFx.Infrastructure;
using LogixDb.Cli.Services;
using LogixDb.Core.Abstractions;
using LogixDb.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace LogixDb.Cli;

public static class App
{
    public static async Task<int> Main()
    {
        return await new CliApplicationBuilder()
            .SetTitle("logix.db")
            .SetDescription(
                "A command-line tool for transforming Rockwell Logix controller projects to a SQL database.")
            .SetExecutableName("ldb")
            .UseConsole(new SystemConsole())
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(commands =>
            {
                var services = new ServiceCollection();
                services.AddTransient<ILogixDatabaseFactory, DatabaseProvider>();
                services.AddLogixSqlite();

                foreach (var commandType in commands)
                    services.AddTransient(commandType);

                return services.BuildServiceProvider();
            })
            .Build()
            .RunAsync();
    }
}