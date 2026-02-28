using LogixDb.Service.Common;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LogixDb.Service.Tests.Common;

[TestFixture]
public class SourceInfoTests
{
    [Test]
    public void Create_L5XFile_ShouldReturnCorrectSourceInfo()
    {
        var sourceInfo = SourceInfo.Create("test.L5X", "C:\\Temp");

        Assert.Multiple(() =>
        {
            Assert.That(sourceInfo.SourceId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(sourceInfo.FileType, Is.EqualTo(FileType.L5X));
            Assert.That(sourceInfo.FileName, Is.EqualTo("test.L5X"));
            Assert.That(sourceInfo.FilePath, Does.StartWith("C:\\Temp"));
            Assert.That(sourceInfo.FilePath, Does.EndWith(".L5X"));
        });
    }

    [Test]
    public void Create_ACDFile_ShouldReturnCorrectSourceInfo()
    {
        var sourceInfo = SourceInfo.Create("test.ACD", "C:\\Temp");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sourceInfo.FileType, Is.EqualTo(FileType.ACD));
            Assert.That(sourceInfo.FilePath, Does.EndWith(".ACD"));
        }
    }
}