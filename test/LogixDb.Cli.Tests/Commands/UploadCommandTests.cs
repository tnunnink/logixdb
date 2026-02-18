using CliFx.Infrastructure;
using LogixDb.Cli.Commands;
using LogixDb.Cli.Common;
using LogixDb.Core.Common;
using LogixDb.Testing;

namespace LogixDb.Cli.Tests.Commands;

[TestFixture]
public class UploadCommandTests : TestDbFixture
{
    [SetUp]
    public async Task Setup()
    {
        await Database.Migrate();
    }

    [Test]
    public async Task Import_FileNotFound_ShouldReturnExpectedErrorCode()
    {
        using var console = new FakeInMemoryConsole();
        var app = TestApp.Create<UploadCommand>(console);

        var exitCode = await app.RunAsync([
            "import", "-c", DbConnection,
            "-s", "Test.L5X"
        ]);

        Assert.That(exitCode, Is.EqualTo(ErrorCodes.FileNotFound));
        await Verify(console.ReadErrorString());
    }

    [Test]
    public async Task Import_WithValidFile_ShouldImportSuccessfully()
    {
        var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.L5X");

        using var console = new FakeInMemoryConsole();
        var app = TestApp.Create<UploadCommand>(console);

        var exitCode = await app.RunAsync([
            "import", "-c", DbConnection,
            "-s", testFile
        ]);

        Assert.That(exitCode, Is.Zero);
    }

    [Test]
    public async Task Import_WithTargetOverride_ShouldUseCustomTarget()
    {
        var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.L5X");

        using var console = new FakeInMemoryConsole();
        var app = TestApp.Create<UploadCommand>(console);

        var exitCode = await app.RunAsync([
            "import", "-c", DbConnection,
            "-s", testFile,
            "-t", "Controller://CustomTarget"
        ]);

        Assert.That(exitCode, Is.Zero);

        var snapshots = await Database.ListSnapshots();
        Assert.That(snapshots.First().TargetKey, Is.EqualTo("Controller://CustomTarget"));
    }

    [Test]
    public async Task Import_WithReplaceLatestAction_ShouldReplaceLatest()
    {
        var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.L5X");

        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        using var console = new FakeInMemoryConsole();
        var app = TestApp.Create<UploadCommand>(console);

        var exitCode = await app.RunAsync([
            "import", "-c", DbConnection,
            "-s", testFile,
            "-a", "ReplaceLatest"
        ]);

        Assert.That(exitCode, Is.Zero);
    }
}