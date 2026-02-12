using FluentAssertions;
using L5Sharp.Core;
using LogixDb.Core.Common;
using LogixDb.Testing;

namespace LogixDb.Core.Tests;

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
            SourceHash = "abc123def456",
            SourceData = [1, 2, 3, 4, 5]
        };

        snapshot.SnapshotId.Should().Be(1);
        snapshot.TargetType.Should().Be("Controller");
        snapshot.TargetName.Should().Be("TestController");
        snapshot.IsPartial.Should().BeFalse();
        snapshot.SourceHash.Should().Be("abc123def456");
        snapshot.SourceData.Should().NotBeEmpty();
    }

    [Test]
    public void Snapshot_CreateFromFakeSource_ShouldHaveExpectedFields()
    {
        var source = TestSource.Load(@"C:\Users\tnunn\Documents\L5X\Test.L5X");

        var snapshot = Snapshot.Create(source);

        snapshot.Should().NotBeNull();
    }
}