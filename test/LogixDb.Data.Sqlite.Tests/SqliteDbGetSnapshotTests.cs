using LogixDb.Testing;

namespace LogixDb.Data.Sqlite.Tests;

[TestFixture]
public class SqliteDbGetSnapshotTests : SqliteTestFixture
{
    [SetUp]
    protected async Task Setup()
    {
        await Database.Migrate();
    }

    [Test]
    public async Task GetLatest_ContainsSingleSnapshot_ShouldNotBeNull()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot);

        var result = await Database.GetSnapshotLatest(snapshot.TargetKey);

        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SnapshotId, Is.EqualTo(snapshot.SnapshotId));
            Assert.That(result.TargetKey, Is.EqualTo(snapshot.TargetKey));
        }
    }
    
    [Test]
    public async Task GetLatest_ContainsMultipleSnapshots_ShouldReturnLatest()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        await Task.Delay(1000); // Ensure different timestamps

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        var result = await Database.GetSnapshotLatest(snapshot1.TargetKey);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.SnapshotId, Is.EqualTo(snapshot2.SnapshotId));
    }

    [Test]
    public async Task GetSnapshotById_ExistingId_ShouldReturnSnapshot()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot);

        var result = await Database.GetSnapshotById(snapshot.SnapshotId);

        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SnapshotId, Is.EqualTo(snapshot.SnapshotId));
            Assert.That(result.TargetKey, Is.EqualTo(snapshot.TargetKey));
        }
    }

    [Test]
    public async Task GetSnapshotById_MultipleSnapshots_ShouldReturnCorrectOne()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        var result = await Database.GetSnapshotById(snapshot1.SnapshotId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.SnapshotId, Is.EqualTo(snapshot1.SnapshotId));
    }
}