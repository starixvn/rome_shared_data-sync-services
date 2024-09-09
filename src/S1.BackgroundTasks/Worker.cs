using System.Diagnostics;
using Hangfire;

namespace S1.BackgroundTasks;

public class Worker(IRecurringJobManager recurringJobManager, ILogger<Worker> logger) : BackgroundService
{
    private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;
    private readonly ILogger<Worker> _logger = logger;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _recurringJobManager.AddOrUpdate("jobId", () => Console.WriteLine("Welcome user in Recurring Job Demo!"), Cron.Minutely);

        return Task.CompletedTask;
    }
}
