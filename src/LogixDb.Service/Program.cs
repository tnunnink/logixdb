using LogixDb.Service.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using LogixConverter.Abstractions;
using LogixConverter.LogixSdk;
using LogixDb.Service.Configuration;
using LogixDb.Service.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService(o => o.ServiceName = "LogixDb");
builder.Services.Configure<LogixConfig>(builder.Configuration.GetSection(nameof(LogixConfig)));
builder.Services.AddSingleton(Channel.CreateUnbounded<SourceInfo>());
builder.Services.AddSingleton<SourceUploadService>();
builder.Services.AddSingleton<ILogixFileConverter, LogixSdkConverter>();
builder.Services.AddLogixDb(builder.Configuration.GetSection(nameof(LogixConfig)).Get<LogixConfig>());
builder.Services.AddHostedService<SourceIngestionService>();

// Add the FtacMonitor service if enabled in configuration. By default, it is disabled. Users need to opt in.
if (builder.Configuration.GetSection(nameof(LogixConfig)).Get<LogixConfig>()?.FtacService.Enabled is true)
{
    builder.Services.AddHostedService<FtacMonitorService>();
}

var app = builder.Build();

// Defines a simple health check endpoint to verify communication with service.
app.MapGet("/health",
    () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow })
);

// Defines the primary endpoint for ingesting L5X and ACD files.
// This lets external tools or scripts upload logix files to the local server.
// The service hosts a background service that will ingest these uploaded files to the configured LogixDb database.
app.MapPost("/ingest", async ([FromForm] IFormFile file, HttpRequest request, SourceUploadService uploadService) =>
{
    if (file.Length == 0)
        return Results.BadRequest("No file content provided.");

    if (!file.FileName.IsLogixFile())
        return Results.BadRequest("Invalid file type. Only .L5X and .ACD files are supported.");

    // Extract custom metadata from Request Headers.
    // This will let users associate custom info with a given source upload.
    const string metadataHeaderPrefix = "Logix-";
    var metadata = new Dictionary<string, string>();
    foreach (var header in request.Headers)
    {
        if (header.Key.StartsWith(metadataHeaderPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var key = header.Key[metadataHeaderPrefix.Length..];
            metadata[key] = header.Value!;
        }
    }

    var source = await uploadService.UploadAsync(file, metadata);

    return Results.Accepted(uri: string.Empty, value: new
    {
        traceId = source.SourceId,
        received = source.FileName,
        status = "Queued"
    });
}).DisableAntiforgery();

await app.RunAsync();

public partial class Program { }