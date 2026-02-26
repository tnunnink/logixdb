namespace LogixDb.Service.Services;

public class FtacMonitorService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //todo implement using SqlDependency
        return Task.CompletedTask;
    }
}