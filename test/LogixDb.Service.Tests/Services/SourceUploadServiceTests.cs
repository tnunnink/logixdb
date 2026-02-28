using System.Threading.Channels;
using LogixDb.Service.Common;
using LogixDb.Service.Configuration;
using LogixDb.Service.Workers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LogixDb.Service.Tests.Services;

[TestFixture]
public class SourceUploadServiceTests
{
    private Mock<IOptions<LogixConfig>> _optionsMock;
    private Mock<ILogger<SourceUploadService>> _loggerMock;
    private Channel<SourceInfo> _channel;
    private SourceUploadService _service;
    private string _testDropPath;

    [SetUp]
    public void Setup()
    {
        _testDropPath = Path.Combine(Path.GetTempPath(), "LogixDbTests", Guid.NewGuid().ToString("N"));
        _optionsMock = new Mock<IOptions<LogixConfig>>();
        _optionsMock.Setup(o => o.Value).Returns(new LogixConfig
        {
            IngestionService = new IngestionConfig { DropPath = _testDropPath }
        });

        _loggerMock = new Mock<ILogger<SourceUploadService>>();
        _channel = Channel.CreateUnbounded<SourceInfo>();
        _service = new SourceUploadService(_channel, _optionsMock.Object, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDropPath))
        {
            Directory.Delete(_testDropPath, true);
        }
    }

    [Test]
    public async Task UploadAsync_ValidFile_ShouldUploadAndQueue()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.L5X");
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock
            .Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream s, CancellationToken t) => s.WriteAsync("test content"u8.ToArray(), t).AsTask());

        var metadata = new Dictionary<string, string> { { "TestKey", "TestValue" } };

        var result = await _service.UploadAsync(fileMock.Object, metadata);
        Assert.Multiple(() =>
        {
            Assert.That(result.FileName, Is.EqualTo("test.L5X"));
            Assert.That(File.Exists(result.FilePath), Is.True);
        });

        var queued = await _channel.Reader.ReadAsync();
        Assert.Multiple(() =>
        {
            Assert.That(queued.SourceId, Is.EqualTo(result.SourceId));
            Assert.That(queued.Metadata["TestKey"], Is.EqualTo("TestValue"));
        });
    }
}