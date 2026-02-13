using CliFx;
using CliFx.Infrastructure;
using LogixDb.Cli.Common;

namespace LogixDb.Cli;

public static class App
{
    public static async Task<int> Main()
    {
        return await new CliApplicationBuilder()
            .SetTitle("LogixDb")
            .SetDescription(
                "A command-line tool for transforming Rockwell Logix controller projects to a SQL database.")
            .SetExecutableName("logixdb")
            .UseConsole(new SystemConsole())
            .AddCommandsFromThisAssembly()
            .AddLogixDb()
            .Build()
            .RunAsync();
    }
}