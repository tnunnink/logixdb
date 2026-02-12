using System.Diagnostics;
using LogixDb.Core.Common;
using LogixDb.Testing;
using NUnit.Framework.Legacy;

namespace LogixDb.Sqlite.Tests;

[TestFixture]
public class SqliteDatabaseTests() : SqliteTestFixture("logix_test.db")
{
    [Test]
    public void EnsureThatWeCanBuildTheTestDatabaseUsingBaseClassServiceProviderAndFactory()
    {
        var database = BuildDatabase();
        Assert.That(database, Is.Not.Null);
        FileAssert.Exists(DataSource);
    }

    [Test]
    public async Task Import_LocalTestSource_ShouldReturnValidId()
    {
        var database = BuildDatabase();
        var snapshot = Snapshot.Create(TestSource.LocalTest());

        var result = await database.Import(snapshot);
        
        Assert.That(result.SnapshotId, Is.GreaterThan(0));
    }
    
    [Test]
    public async Task Import_LocalExampleSource_ShouldReturnValidId()
    {
        var database = BuildDatabase();
        var snapshot = Snapshot.Create(TestSource.LocalExample());

        var stopwatch = Stopwatch.StartNew();
        var result = await database.Import(snapshot);
        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        Assert.That(result.SnapshotId, Is.GreaterThan(0));
    }
}