using CliFx.Infrastructure;
using L5Sharp.Core;
using LogixDb.Cli.Commands;
using LogixDb.Cli.Common;
using LogixDb.Core.Common;
using LogixDb.Testing;
using Task = System.Threading.Tasks.Task;

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
    public async Task Upload_FileNotFound_ShouldReturnExpectedErrorCode()
    {
        using var console = new FakeInMemoryConsole();
        var app = TestApp.Create<UploadCommand>(console);

        var exitCode = await app.RunAsync([
            "upload", "-c", DbConnection,
            "-s", "Fake.L5X"
        ]);

        Assert.That(exitCode, Is.EqualTo(ErrorCodes.FileNotFound));
    }

    [Test]
    public async Task Upload_WithValidFile_ShouldImportSuccessfully()
    {
        //Generate and save L5X to the local directory for command.
        var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.L5X");
        var source = TestSource.Fake();
        source.Save(testFile);

        using var console = new FakeInMemoryConsole();
        var app = TestApp.Create<UploadCommand>(console);

        var exitCode = await app.RunAsync([
            "upload", "-c", DbConnection,
            "-s", testFile
        ]);

        Assert.That(exitCode, Is.Zero);
        File.Delete(testFile);
    }

    [Test]
    public async Task Upload_WithTargetOverride_ShouldUseCustomTarget()
    {
        //Generate and save L5X to the local directory for command.
        var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.L5X");
        var source = TestSource.Fake();
        source.Save(testFile);

        using var console = new FakeInMemoryConsole();
        var app = TestApp.Create<UploadCommand>(console);

        var exitCode = await app.RunAsync([
            "upload", "-c", DbConnection,
            "-s", testFile,
            "-t", "Controller://CustomTarget"
        ]);

        Assert.That(exitCode, Is.Zero);

        var snapshots = await Database.ListSnapshots();
        Assert.That(snapshots.First().TargetKey, Is.EqualTo("Controller://CustomTarget"));
        File.Delete(testFile);
    }

    [Test]
    public async Task Upload_WithReplaceLatestAction_ShouldReplaceLatest()
    {
        //Generate and save L5X to the local directory for command.
        var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.L5X");
        var source = TestSource.Fake();
        source.Save(testFile);

        var snapshot1 = Snapshot.Create(TestSource.LocalTest(), "TestTarget");
        await Database.AddSnapshot(snapshot1);

        using var console = new FakeInMemoryConsole();
        var app = TestApp.Create<UploadCommand>(console);

        var exitCode = await app.RunAsync([
            "upload", "-c", DbConnection,
            "-s", testFile,
            "-t", "TestTarget",
            "-a", "ReplaceLatest"
        ]);

        Assert.That(exitCode, Is.Zero);
    }
}