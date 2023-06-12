using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace AutoTransfer.Jobs;

public class BaseAutoTransferJob : BackgroundServiceBase
{
    public readonly JobsSettingsModel settings;
    public readonly ITransferService transferService;
    private const int UMDelayCheckingTimeInMilliseconds = 60000;

    public BaseAutoTransferJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        ITransferService transferService,
        IOptions<JobsSettingsModel> settings)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
        this.settings = settings.Value;
        this.transferService = transferService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (scheduleJob.IsActive)
        {
            await loggerService.Info($"{scheduleJob.JobInfo} is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteInternal(stoppingToken);
                    await Task.Delay(settings.StartJobCheckingIntervalInMilliSeconds, stoppingToken);
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

    public bool IsRunTime(TimeOnly runningTime, DateTime currentTime)
    {
        return currentTime.Hour == runningTime.Hour && currentTime.Minute == runningTime.Minute;
    }

    public async Task ImportLogs()
    {
        try
        {
            await transferService.ImportLogs(settings.HistoryReplicateNumber);
        }
        catch (Exception ex)
        {
            await HandleException(ex);
        }
    }
}
