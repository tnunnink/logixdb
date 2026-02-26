using FluentAssertions;
using L5Sharp.Core;
using LogixDb.Data.Maps;

namespace LogixDb.Data.Tests;

[TestFixture]
public class TagMapTests
{
    [Test]
    public void GenerateTable_WithValidRecordsAndId_ShouldHaveExpectedRowCount()
    {
        var map = new TagMap();
        List<Tag> tags = [new("First", 1), new("Second", 2), new("Third", 3)];

        var table = map.GenerateTable(tags, 1);

        table.Rows.Count.Should().Be(3);
    }
}