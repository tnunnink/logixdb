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
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.L5X");
        var metadata = new Dictionary<string, string> { { "Key1", "Value1" } };

        var sourceInfo = SourceInfo.Create(fileMock.Object, "C:\\Temp", metadata);

        Assert.Multiple(() =>
        {
            Assert.That(sourceInfo.SourceId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(sourceInfo.FileType, Is.EqualTo(FileType.L5X));
            Assert.That(sourceInfo.FileName, Is.EqualTo("test.L5X"));
            Assert.That(sourceInfo.FilePath, Does.StartWith("C:\\Temp"));
            Assert.That(sourceInfo.FilePath, Does.EndWith(".L5X"));
            Assert.That(sourceInfo.Metadata["Key1"], Is.EqualTo("Value1"));
        });
    }

    [Test]
    public void Create_ACDFile_ShouldReturnCorrectSourceInfo()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.ACD");

        var sourceInfo = SourceInfo.Create(fileMock.Object, "C:\\Temp");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sourceInfo.FileType, Is.EqualTo(FileType.ACD));
            Assert.That(sourceInfo.FilePath, Does.EndWith(".ACD"));
        }
    }
}