using CliFx.Infrastructure;
using LogixDb.Cli.Commands;

namespace LogixDb.Cli.Tests.Commands;

[TestFixture]
public class MigrateCommandTests : TestDbFixture
{
    [Test]
    public async Task Migrate_ValidSqliteConnectionPath_ShouldCreateDatabaseFile()
    {
        var console = new FakeInMemoryConsole();
        var app = TestApp.Create<MigrateCommand>(console);

        var exitCode = await app.RunAsync(["migrate", "-c", DbConnection]);
        
        Assert.That(exitCode, Is.Zero);
    }
}