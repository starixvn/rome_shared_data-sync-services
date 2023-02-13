using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace Shared.Jobs;

[ExcludeFromCodeCoverage]
public abstract class IntervalBackgroundService : BackgroundServiceBase
{
    private const int OneSecond = 1000;
    private const int StartMultiplier = 1;
    private const int TwoMultiplier = 2;
    private const int MaxMultiplier = 10;
    private int multiplier = StartMultiplier;

    protected IntervalBackgroundService(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
    }

    public override async Task ExecuteInternal(CancellationToken stoppingToken)
    {
        var umInfo = await GetUnderMaintenanceTime();

        if (umInfo.IsUnderMaintenanceTime)
        {
            await loggerService.Info($"{scheduleJob.JobInfo} is Under Maintenance from {umInfo.From} to {umInfo.To}.");

            multiplier = umDelayTimeInSeconds;
        }
        else
        {
            multiplier = await ExecuteJobAndCalculateMultiplier(multiplier);
        }

        await Task.Delay(multiplier * OneSecond, stoppingToken);
    }

    private async Task<int> ExecuteJobAndCalculateMultiplier(int multiplier)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var hasData = await ExecuteJob();
            var calculatedMultiplier = CalculateMultiplier(hasData, multiplier);
            sw.Stop();

            var logInfo = hasData
                ? $"Total execution time {sw.ElapsedMilliseconds} milliseconds"
                : "No Data to Import";
            loggerService.InfoConsole(
                "{JobInfo} {logInfo}. SleepTimeInSeconds={calculatedMultiplier}",
                scheduleJob.JobInfo,
                logInfo,
                calculatedMultiplier);

            return calculatedMultiplier;
        }
        catch (Exception ex)
        {
            await HandleException(ex);
        }

        return StartMultiplier;
    }

    private int CalculateMultiplier(bool hasData, int multiplier)
    {
        if (scheduleJob.DelayExecutionTimeInSeconds > 0)
        {
            return scheduleJob.DelayExecutionTimeInSeconds;
        }

        if (hasData)
        {
            return StartMultiplier;
        }

        multiplier *= TwoMultiplier;

        return multiplier > MaxMultiplier
            ? MaxMultiplier
            : multiplier;
    }

    public abstract Task<bool> ExecuteJob();
}
