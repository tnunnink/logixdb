using System.Threading.Channels;
using L5Sharp.Core;
using LogixConverter.Abstractions;
using LogixDb.Data;
using LogixDb.Data.Sqlite;
using LogixDb.Service.Common;
using LogixDb.Service.Configuration;
using LogixDb.Service.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LogixDb.Service.Tests.Services;

[TestFixture]
public class SourceIngestionServiceTests
{
    private Mock<ILogixFileConverter> _fileConverterMock;
    private Mock<IHostApplicationLifetime> _lifetimeMock;
    private Mock<ILogger<SourceIngestionService>> _loggerMock;
    private Mock<IOptions<LogixConfig>> _optionsMock;
    private Channel<SourceInfo> _channel;
    private string _testDbPath;
    private string _testDropPath;
    private SqliteDb _logixDb;

    [SetUp]
    public void Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"LogixDb_{Guid.NewGuid():N}.db");
        _testDropPath = Path.Combine(Path.GetTempPath(), "LogixDbUploads", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDropPath);

        var connectionInfo = SqlConnectionInfo.Parse(_testDbPath);
        _logixDb = new SqliteDb(connectionInfo);
        // We need to migrate the database for it to be valid
        _logixDb.Migrate().GetAwaiter().GetResult();

        _fileConverterMock = new Mock<ILogixFileConverter>();
        _lifetimeMock = new Mock<IHostApplicationLifetime>();
        _loggerMock = new Mock<ILogger<SourceIngestionService>>();
        _optionsMock = new Mock<IOptions<LogixConfig>>();
        _optionsMock.Setup(o => o.Value).Returns(new LogixConfig
        {
            IngestionService = new IngestionConfig
            {
                DbConnection = _testDbPath,
                DropPath = _testDropPath,
                OnImport = SnapshotAction.Append
            }
        });

        _channel = Channel.CreateUnbounded<SourceInfo>();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testDbPath)) File.Delete(_testDbPath);
        if (Directory.Exists(_testDropPath)) Directory.Delete(_testDropPath, true);
    }

    [Test]
    public async System.Threading.Tasks.Task ExecuteAsync_ValidSource_ShouldProcessAndAddToDb()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var fileName = "test.L5X";
        var filePath = Path.Combine(_testDropPath, $"{sourceId:N}.L5X");

        // Create a dummy L5X file
        var l5x = L5X.Parse(
            "<RSLogix5000Content TargetName=\"TestController\" TargetType=\"Controller\" SchemaRevision=\"1.0\"></RSLogix5000Content>");
        l5x.Save(filePath);

        var source = new SourceInfo
        {
            SourceId = sourceId,
            FileType = FileType.L5X,
            FileName = fileName,
            FilePath = filePath
        };

        var service = new SourceIngestionService(
            _channel,
            _logixDb,
            _fileConverterMock.Object,
            _lifetimeMock.Object,
            _optionsMock.Object,
            _loggerMock.Object);

        using var cts = new CancellationTokenSource();
        var startTask = service.StartAsync(cts.Token);

        // Act
        await _channel.Writer.WriteAsync(source);

        // Wait a bit for processing
        await System.Threading.Tasks.Task.Delay(1000);

        // Assert
        var snapshots = await _logixDb.ListSnapshots();
        Assert.That(snapshots.Count(), Is.EqualTo(1));
        Assert.That(snapshots.First().TargetName, Is.EqualTo("TestController"));

        Assert.That(File.Exists(filePath), Is.False, "Original file should be deleted");

        await service.StopAsync(cts.Token);
        await startTask;
    }

    [Test]
    public async System.Threading.Tasks.Task ExecuteAsync_MigrationRequired_ShouldShutdownApp()
    {
        // Arrange
        // Create a new DB but don't migrate it
        var unmigratedDbPath = Path.Combine(Path.GetTempPath(), $"LogixDb_Unmigrated_{Guid.NewGuid():N}.db");
        // Create an empty file to simulate unmigrated DB
        File.WriteAllBytes(unmigratedDbPath, []);

        var connectionInfo = SqlConnectionInfo.Parse(unmigratedDbPath);
        var unmigratedDb = new SqliteDb(connectionInfo);

        var service = new SourceIngestionService(
            _channel,
            unmigratedDb,
            _fileConverterMock.Object,
            _lifetimeMock.Object,
            _optionsMock.Object,
            _loggerMock.Object);

        // Act
        var startTask = service.StartAsync(CancellationToken.None);
        // Add a delay to ensure the background task has time to run and call StopApplication
        await System.Threading.Tasks.Task.Delay(500);
        await startTask;

        // Assert
        _lifetimeMock.Verify(l => l.StopApplication(), Times.Once);

        if (File.Exists(unmigratedDbPath)) File.Delete(unmigratedDbPath);
    }
}