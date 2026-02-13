using System.Diagnostics;
using LogixDb.Core.Common;
using LogixDb.Testing;
using NUnit.Framework.Legacy;

namespace LogixDb.Sqlite.Tests;

[TestFixture]
public class SqliteDatabaseTests() : SqliteTestFixture("logix_test.db")
{
    [Test]
    public async Task CreateOrConnect_WhenCalledOnValidDatabase_FileShouldExist()
    {
        var database = ResolveDatabase();

        await database.Build();

        FileAssert.Exists(DataSource);
    }

    [Test]
    public async Task Import_LocalTestSource_ShouldReturnValidId()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        var database = ResolveDatabase();
        await database.Build();

        var result = await database.Import(snapshot);

        Assert.That(result.SnapshotId, Is.GreaterThan(0));
    }

    [Test]
    [Explicit("Requires local Example.L5X file - run manually only")]
    public async Task Import_LocalExampleSource_ShouldReturnValidId()
    {
        var snapshot = Snapshot.Create(TestSource.LocalExample());
        var database = ResolveDatabase();
        await database.Build();

        var stopwatch = Stopwatch.StartNew();
        var result = await database.Import(snapshot);
        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        Assert.That(result.SnapshotId, Is.GreaterThan(0));
    }
}