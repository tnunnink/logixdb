using LogixDb.Testing;

namespace LogixDb.Data.Sqlite.Tests;

[TestFixture]
public class SqliteDbListSnapshotTests : SqliteTestFixture
{
    [SetUp]
    protected async Task Setup()
    {
        await Database.Migrate();
    }
    
    [Test]
    public async Task ListSnapshots_EmptyDatabase_ShouldReturnEmpty()
    {
        var result = (await Database.ListSnapshots()).ToArray();

        Assert.That(result, Is.Empty);
    }
    
    [Test]
    public async Task ListSnapshots_HasSingleSnapshot_ShouldHaveExpectedCount()
    {
        await Database.AddSnapshot(Snapshot.Create(TestSource.LocalTest()));

        var result = (await Database.ListSnapshots()).ToArray();

        Assert.That(result, Has.Length.EqualTo(1));
    }
    
    [Test]
    public async Task ListSnapshots_MultipleSnapshots_ShouldHaveExpectedCount()
    {
        await Database.AddSnapshot(Snapshot.Create(TestSource.LocalTest()));
        await Database.AddSnapshot(Snapshot.Create(TestSource.LocalTest()));
        await Database.AddSnapshot(Snapshot.Create(TestSource.LocalTest()));

        var result = (await Database.ListSnapshots()).ToArray();

        Assert.That(result, Has.Length.EqualTo(3));
    }
    
    [Test]
    public async Task ListSnapshots_FilterByTargetKey_ShouldReturnMatchingOnly()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest(), "Controller://DifferentTarget");
        await Database.AddSnapshot(snapshot2);

        var result = (await Database.ListSnapshots(snapshot1.TargetKey)).ToArray();

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].TargetKey, Is.EqualTo(snapshot1.TargetKey));
    }
    
    [Test]
    public async Task ListSnapshots_FilterByNonExistentTargetKey_ShouldReturnEmpty()
    {
        await Database.AddSnapshot(Snapshot.Create(TestSource.LocalTest()));

        var result = (await Database.ListSnapshots("nonexistent://target")).ToArray();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ListSnapshots_MultipleSnapshotsSameTarget_ShouldReturnAll()
    {
        var snapshot1 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot1);

        var snapshot2 = Snapshot.Create(TestSource.LocalTest());
        await Database.AddSnapshot(snapshot2);

        var result = (await Database.ListSnapshots(snapshot1.TargetKey)).ToArray();

        Assert.That(result, Has.Length.EqualTo(2));
    }
}