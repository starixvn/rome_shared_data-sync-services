using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace AutoTransfer.Jobs;

public class BaseAutoTransferJob : BackgroundServiceBase
{
    public BaseAutoTransferJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
    }

    public override async Task ExecuteInternal(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
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
