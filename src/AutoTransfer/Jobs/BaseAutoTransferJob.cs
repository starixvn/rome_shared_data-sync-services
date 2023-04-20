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

    public async Task WaitForBalanceUpProcess(CancellationToken stoppingToken)
    {
        while (true)
        {
            var endTime = await transferService.CheckBalanceUpProcess();

            if (endTime == null)
            {
                break;
            }

            var currentTime = DateTime.Now;
            var timeDelay = endTime.Value - currentTime;
            loggerService.InfoConsole($"{scheduleJob.JobInfo} delay to {timeDelay}");
            await Task.Delay(timeDelay, stoppingToken);
        }
    }

    public bool IsRunTime()
    {
        if (!scheduleJob.DelayExecutionAtExactlyTime.HasValue)
        {
            return false;
        }

        var currentTime = DateTime.Now;
        var scheduleTime = scheduleJob.DelayExecutionAtExactlyTime.Value;

        return currentTime.Hour == scheduleTime.Hour && currentTime.Minute == scheduleTime.Minute;
    }

    public async virtual Task WaitingToNextSchedule(CancellationToken stoppingToken)
    {
        var ct = DateTime.Now;
        var executionTime = scheduleJob.DelayExecutionAtExactlyTime.Value;
        var todayExecutionTime = new DateTime(ct.Year, ct.Month, ct.Day, executionTime.Hour, executionTime.Minute, 0);
        var runTime = todayExecutionTime > ct ? todayExecutionTime : todayExecutionTime.AddDays(1);

        var timeDelay = runTime - ct;
        loggerService.InfoConsole("{JobInfo} delay to {next}", scheduleJob.JobInfo, timeDelay);
        await Task.Delay(timeDelay, stoppingToken);
    }

    public async virtual Task WaitingToSchedule(CancellationToken stoppingToken)
    {
        if (!scheduleJob.DelayExecutionAtExactlyDateTime.HasValue)
        {
            await StopAsync(stoppingToken);
            return;
        }

        var ct = DateTime.Now;
        var currentTime = new DateTime(ct.Year, ct.Month, ct.Day, ct.Hour, ct.Minute, 0);

        var executionTime = scheduleJob.DelayExecutionAtExactlyDateTime.Value;
        var todayExecutionTime = new DateTime(ct.Year, ct.Month, ct.Day, executionTime.Hour, executionTime.Minute, 0);

        if (todayExecutionTime == currentTime)
        {
            return;
        }

        if (todayExecutionTime < currentTime)
        {
            await StopAsync(stoppingToken);
            return;
        }

        var timeDelay = todayExecutionTime - ct;
        loggerService.InfoConsole("{JobInfo} delay to {next}", scheduleJob.JobInfo, timeDelay);
        await Task.Delay(timeDelay, stoppingToken);
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
