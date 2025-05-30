using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;
using ResetPotentialWinLimit.Models;
using ResetPotentialWinLimit.Services;

namespace ResetPotentialWinLimit.Jobs;

public class BaseResetPotentialWinLimitJob : BackgroundServiceBase
{
    public readonly JobsSettingsModel settings;
    public readonly IResetPotentialWinLimitService resetPotentialWinLimitService;
    private const int UMDelayCheckingTimeInMilliseconds = 60000;

    public BaseResetPotentialWinLimitJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IResetPotentialWinLimitService resetPotentialWinLimitService,
        IOptions<JobsSettingsModel> settings)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
        this.settings = settings.Value;
        this.resetPotentialWinLimitService = resetPotentialWinLimitService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (scheduleJob.IsActive)
        {
            await loggerService.Error(new Exception($"{scheduleJob.JobInfo} is running."), scheduleJob, nameof(ExecuteAsync));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteInternal(stoppingToken);
                }
                catch (Exception e)
                {
                    await HandleException(e);
                }
            }
        }
    }

    public override async Task ExecuteInternal(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    public async Task WaitForSystemUM(CancellationToken stoppingToken)
    {
        while (true)
        {
            var umInfo = await GetUnderMaintenanceTime();

            if (!umInfo.IsUnderMaintenanceTime)
            {
                break;
            }

            await Task.Delay(UMDelayCheckingTimeInMilliseconds, stoppingToken);
        }
    }

    public Task Notify(string message)
    {
        var siteDBName = string.IsNullOrWhiteSpace(settings.SiteDBName)
            ? string.Empty
            : $" - [{settings.SiteDBName}]";

        return loggerService.Notify($"{scheduleJob.JobInfo}{siteDBName} {message}", settings.NotifyCategory);
    }

    public bool IsRunTime(TimeOnly runningTime, DateTime currentTime)
    {
        return currentTime.Hour == runningTime.Hour && currentTime.Minute == runningTime.Minute;
    }
}
