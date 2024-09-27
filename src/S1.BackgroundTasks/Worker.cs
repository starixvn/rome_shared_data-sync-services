using System.Text.Json;
using Hangfire;
using S1.BackgroundTasks.Helpers;
using S1.BackgroundTasks.Services;

namespace S1.BackgroundTasks;

public class Worker(
    IConfiguration configuration,
    IPlayerWinLossService playerWinLossService,
    IWinLossImportService winLossImportService,
    IRecurringJobManager recurringJobManager,
    Fanex.Logging.ILogger logger) : BackgroundService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IPlayerWinLossService _playerWinLossService = playerWinLossService;
    private readonly IWinLossImportService _winLossImportService = winLossImportService;
    private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;
    private readonly Fanex.Logging.ILogger _logger = logger;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _recurringJobManager.AddOrUpdate("jobId", () => SyncPlayerWinLossData(), _configuration.GetValue<string>("CronExpression"));
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return _logger.ErrorAsync(ex.Message, ex);
        }
    }

    public async Task SyncPlayerWinLossData()
    {
        try
        {
            var schedules = await _winLossImportService.GetSchedules();
            await _logger.InfoAsync(JsonSerializer.Serialize(schedules));
            if (schedules != null && schedules.Any())
            {
                foreach (var schedule in schedules)
                {
                    DateTimeOffset fromDate = DateTime.SpecifyKind(schedule.FromDate, DateTimeKind.Local);
                    DateTimeOffset toDate = DateTime.SpecifyKind(schedule.ToDate, DateTimeKind.Local);
                    await _logger.InfoAsync($"Start sync player winloss from {fromDate} to {toDate}");
                    var winlossData = await RetryHelpers.Retry(() => _playerWinLossService.GetPlayerWinLoss(fromDate, toDate));
                    await RetryHelpers.Retry(async () =>
                    {
                        await _winLossImportService.ImportPlayerWinLoss(schedule.FromDate, schedule.ToDate, winlossData);
                        await _logger.InfoAsync($"Completed sync player winloss from {fromDate} to {toDate}");
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(ex.Message, ex);
        }
    }
}
