using LogixDb.Cli.Commands.Database;


namespace LogixDb.Cli.Tests.Commands.Database;

[TestFixture]
public class BuildCommandTests
{
    [Test]
    public async Task Build_ValidSqliteConnectionPath_ShouldCreateDatabaseFile()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        var app = TestApp.Create<BuildCommand>(out _);

        var exitCode = await app.RunAsync(["build", "-c", tempPath]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exitCode, Is.Zero);
            Assert.That(File.Exists(tempPath), Is.True);
        }

        if (File.Exists(tempPath)) File.Delete(tempPath);
    }
}