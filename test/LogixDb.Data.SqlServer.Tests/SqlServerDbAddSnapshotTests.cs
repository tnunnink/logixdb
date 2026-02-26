using System.Diagnostics;
using Dapper;
using LogixDb.Testing;
using Task = System.Threading.Tasks.Task;

namespace LogixDb.Data.SqlServer.Tests;

[TestFixture]
public class SqlServerDbAddSnapshotTests : SqlServerTestFixture
{
    [SetUp]
    protected async Task Setup()
    {
        await Database.Migrate();
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

    [Test]
    public async Task AddSnapshot_WithAppend_ShouldKeepAllSnapshots()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task AddSnapshot_WithReplaceLatest_ShouldReplaceOnlyLatest()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        await Task.Delay(1000); // Ensure different timestamps

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        await Task.Delay(1000);

        var snapshot3 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot3, SnapshotAction.ReplaceLatest);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));

        var snapshots = result.OrderBy(s => s.SnapshotId).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshots[0].SnapshotId, Is.EqualTo(snapshot1.SnapshotId));
            Assert.That(snapshots[1].SnapshotId, Is.EqualTo(snapshot3.SnapshotId));
        }
    }

    [Test]
    public async Task AddSnapshot_WithReplaceLatestOnSingleSnapshot_ShouldReplaceIt()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2, SnapshotAction.ReplaceLatest);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].SnapshotId, Is.EqualTo(snapshot2.SnapshotId));
    }

    [Test]
    public async Task AddSnapshot_WithReplaceAll_ShouldRemoveAllPreviousSnapshots()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        var snapshot3 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot3, SnapshotAction.ReplaceAll);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].SnapshotId, Is.EqualTo(snapshot3.SnapshotId));
    }

    [Test]
    public async Task AddSnapshot_WithReplaceLatestDifferentTargets_ShouldOnlyAffectSameTarget()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest(), "Controller://CustomTarget");
        await Database.AddSnapshot(snapshot2);

        var snapshot3 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot3, SnapshotAction.ReplaceLatest);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task AddSnapshot_WithReplaceAllDifferentTargets_ShouldOnlyAffectSameTarget()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        var snapshot3 = Snapshot.Create(TestSource.LocalTest(), "Controller://CustomTarget");
        await Database.AddSnapshot(snapshot3);

        var snapshot4 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot4, SnapshotAction.ReplaceAll);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(2));

        var testSnapshots = result.Where(s => s.TargetKey == snapshot1.TargetKey).ToArray();
        Assert.That(testSnapshots, Has.Length.EqualTo(1));
        Assert.That(testSnapshots[0].SnapshotId, Is.EqualTo(snapshot4.SnapshotId));

        var exampleSnapshots = result.Where(s => s.TargetKey == snapshot3.TargetKey).ToArray();
        Assert.That(exampleSnapshots, Has.Length.EqualTo(1));
        Assert.That(exampleSnapshots[0].SnapshotId, Is.EqualTo(snapshot3.SnapshotId));
    }

    [Test]
    public async Task AddSnapshot_MultipleTimes_ShouldSetImportDate()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot);

        using var connection = await Database.Connect();
        var importDate = await connection.QuerySingleAsync<DateTime>(
            "SELECT import_date FROM snapshot WHERE snapshot_id = @id",
            new { id = snapshot.SnapshotId }
        );

        Assert.That(importDate, Is.GreaterThan(DateTime.MinValue));
        Assert.That(importDate, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    /*[Test]
    public async Task AddSnapshot_ShouldPopulateTargetTable()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot);

        await AssertRecordExists("target", "target_key", snapshot.TargetKey);
    }

    [Test]
    public async Task AddSnapshot_WithSameTargetTwice_ShouldReuseSameTargetId()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        await AssertRecordCount("target", 1);
    }

    [Test]
    public async Task AddSnapshot_FakeSource_ShouldContainExpectedNumberOFDataTypesRecords()
    {
        var snapshot = Snapshot.Create(TestSource.Custom(c =>
        {
            c.DataTypes.Add(new DataType("TestType") { Description = "This is a test" });
        }));

        await Database.AddSnapshot(snapshot);

        await AssertRecordExists("data_type", "type_name", "TestType");
    }*/
}