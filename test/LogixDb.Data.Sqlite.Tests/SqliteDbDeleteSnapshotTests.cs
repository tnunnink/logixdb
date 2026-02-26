using LogixDb.Testing;

namespace LogixDb.Data.Sqlite.Tests;

[TestFixture]
public class SqliteDbDeleteSnapshotTests : SqliteTestFixture
{
    [SetUp]
    protected async Task Setup()
    {
        await Database.Migrate();
    }

    [Test]
    public async Task DeleteSnapshot_ById_ShouldRemoveSnapshot()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot);

        await Database.DeleteSnapshot(snapshot.SnapshotId);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task DeleteSnapshot_ById_ShouldOnlyRemoveSpecifiedSnapshot()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        await Database.DeleteSnapshot(snapshot1.SnapshotId);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].SnapshotId, Is.EqualTo(snapshot2.SnapshotId));
    }

    [Test]
    public async Task DeleteSnapshotsFor_ExistingTarget_ShouldRemoveAllForTarget()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        var snapshot3 = Snapshot.Create(TestSource.LocalTest(), "Controller://CustomTarget");
        await Database.AddSnapshot(snapshot3);

        await Database.DeleteSnapshotsFor(snapshot1.TargetKey);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].TargetKey, Is.EqualTo(snapshot3.TargetKey));
    }

    [Test]
    public async Task DeleteSnapshotsFor_NonExistentTarget_ShouldNotAffectOthers()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot);

        await Database.DeleteSnapshotsFor("nonexistent://target");

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task DeleteSnapshotLatest_SingleSnapshot_ShouldRemoveIt()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot);

        await Database.DeleteSnapshotLatest(snapshot.TargetKey);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task DeleteSnapshotLatest_MultipleSnapshots_ShouldOnlyRemoveLatest()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        await Task.Delay(1000); // Ensure different timestamps

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        await Database.DeleteSnapshotLatest(snapshot1.TargetKey);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].SnapshotId, Is.EqualTo(snapshot1.SnapshotId));
    }

    [Test]
    public async Task DeleteSnapshotsBefore_DateInPast_ShouldNotRemoveAnything()
    {
        var snapshot = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot);

        await Database.DeleteSnapshotsBefore(DateTime.Now.AddDays(-2));

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task DeleteSnapshotsBefore_DateInFuture_ShouldRemoveAll()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest(), "Controller://CustomTarget");
        await Database.AddSnapshot(snapshot2);

        await Database.DeleteSnapshotsBefore(DateTime.Now.AddDays(1));

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task DeleteSnapshotsBefore_WithTargetKey_ShouldOnlyAffectSpecifiedTarget()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest(), "Controller://CustomTarget");
        await Database.AddSnapshot(snapshot2);

        await Database.DeleteSnapshotsBefore(DateTime.Now.AddDays(1), snapshot1.TargetKey);

        var result = (await Database.ListSnapshots()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].TargetKey, Is.EqualTo(snapshot2.TargetKey));
    }
}