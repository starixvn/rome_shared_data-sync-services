using Hangfire;

namespace S1.BackgroundTasks;

public class Worker(IRecurringJobManager recurringJobManager, Fanex.Logging.ILogger logger) : BackgroundService
{
    private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;
    private readonly Fanex.Logging.ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _recurringJobManager.AddOrUpdate("jobId", () => Console.WriteLine("Welcome user in Recurring Job Demo!"), Cron.Minutely);
            }

        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(ex.Message, ex);
        }
    }
}
