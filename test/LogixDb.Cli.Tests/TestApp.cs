using CliFx;
using CliFx.Infrastructure;
using LogixDb.Cli.Common;

namespace LogixDb.Cli.Tests;

/// <summary>
/// Helper class for creating configured CLI applications for testing purposes.
/// </summary>
public static class TestApp
{
    /// <summary>
    /// Creates a CLI application configured for testing with the specified command.
    /// </summary>
    /// <param name="console">The fake console instance used to capture output during testing.</param>
    /// <typeparam name="TCommand">The command type to register with the CLI application.</typeparam>
    /// <returns>A configured CLI application instance ready for testing.</returns>
    public static CliApplication Create<TCommand>(out FakeInMemoryConsole console) where TCommand : ICommand
    {
        console = new FakeInMemoryConsole();

        return new CliApplicationBuilder()
            .SetTitle("Logix.Cli")
            .SetDescription("Console application providing CLI for Logix projects.")
            .SetExecutableName("logix")
            .UseConsole(console)
            .AddCommand<TCommand>()
            .AddLogixDb()
            .Build();
    }
}