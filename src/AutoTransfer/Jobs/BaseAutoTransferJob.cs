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
        if (settings.IgnoreSystemUM)
        {
            return;
        }

        while (true)
        {
            var umInfo = await GetUnderMaintenanceTime();

            if (!umInfo.IsUnderMaintenanceTime)
            {
                break;
            }

            await loggerService.Info($"{scheduleJob.JobInfo} is Under Maintenance from {umInfo.From} to {umInfo.To}.");

            var endTime = DateTime.Parse(umInfo.To);
            var currentTime = DateTime.Now;
            var timeDelay = endTime - currentTime;
            loggerService.InfoConsole($"{scheduleJob.JobInfo} delay to {timeDelay}");
            await Task.Delay(timeDelay, stoppingToken);
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
        var currentTime = DateTime.Now;
        var executionTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, scheduleJob.DelayExecutionAtExactlyTime.Value.Hour, scheduleJob.DelayExecutionAtExactlyTime.Value.Minute, 0);

        var todayExecutionTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, executionTime.Hour, executionTime.Minute, 0);
        var runTime = todayExecutionTime > currentTime ? todayExecutionTime : todayExecutionTime.AddDays(1);
        var timeDelay = runTime - currentTime;
        loggerService.InfoConsole("{JobInfo} delay to {next}", scheduleJob.JobInfo, timeDelay);

        await Task.Delay(timeDelay, stoppingToken);
    }
}
