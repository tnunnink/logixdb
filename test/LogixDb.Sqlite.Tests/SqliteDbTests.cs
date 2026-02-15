using System.Diagnostics;
using LogixDb.Core.Common;
using LogixDb.Testing;

namespace LogixDb.Sqlite.Tests;

[TestFixture]
public class SqliteDbTests : SqliteTestFixture
{
    [SetUp]
    protected void Setup()
    {
        Database.Migrate();
    }

    [Test]
    public async Task AddSnapshot_LocalTestSource_ShouldReturnValidId()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());

        await Database.AddSnapshot(snapshot);

        Assert.That(snapshot.SnapshotId, Is.GreaterThan(0));
    }

    [Test]
    public async Task AddSnapshot_LocalExampleSource_ShouldReturnValidId()
    {
        var snapshot = Snapshot.Create(TestSource.LocalExample());

        var stopwatch = Stopwatch.StartNew();
        await Database.AddSnapshot(snapshot);
        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        Assert.That(snapshot.SnapshotId, Is.GreaterThan(0));
    }
}