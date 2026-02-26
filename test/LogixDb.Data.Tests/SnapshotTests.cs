using FluentAssertions;
using LogixDb.Testing;

namespace LogixDb.Data.Tests;

[TestFixture]
public class SnapshotTests
{
    [Test]
    public void Snapshot_CanBeCreated_WithRequiredFields()
    {
        var snapshot = new Snapshot
        {
            SnapshotId = 1,
            TargetType = "Controller",
            TargetName = "TestController",
            IsPartial = false,
            ImportDate = DateTime.UtcNow,
            ImportUser = "TestUser",
            ImportMachine = "TestMachine",
            SourceHash = [1, 2, 3, 4, 5],
            SourceData = [1, 2, 3, 4, 5]
        };

        snapshot.SnapshotId.Should().Be(1);
        snapshot.TargetType.Should().Be("Controller");
        snapshot.TargetName.Should().Be("TestController");
        snapshot.IsPartial.Should().BeFalse();
        snapshot.SourceHash.Should().NotBeEmpty();
        snapshot.SourceData.Should().NotBeEmpty();
    }

    [Test]
    public void Snapshot_CreateFromFakeSource_ShouldHaveExpectedFields()
    {
        var source = TestSource.LocalTest();

        var snapshot = Snapshot.Create(source);

        snapshot.Should().NotBeNull();
    }
}