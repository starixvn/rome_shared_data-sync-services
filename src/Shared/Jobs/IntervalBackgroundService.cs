using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Configurations;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace Shared.Jobs;

[ExcludeFromCodeCoverage]
public abstract class IntervalBackgroundService : BackgroundService
{
    private const int OneSecond = 1000;
    private const int StartMultiplier = 1;
    private const int TwoMultiplier = 2;
    private const int MaxMultiplier = 10;

    protected readonly ScheduleJob scheduleJob;
    protected readonly ILoggerService loggerService;
    private readonly IMaintenanceService maintenanceService;
    private readonly int umDelayTimeInSeconds;

    protected IntervalBackgroundService(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration)
    {
        this.scheduleJob = scheduleJob;
        this.loggerService = loggerService;
        this.maintenanceService = maintenanceService;
        umDelayTimeInSeconds = umClientConfiguration.ResultCacheDuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!scheduleJob.IsActive)
        {
            return;
        }

        await loggerService.Info($"{scheduleJob.JobInfo} is running.");
        var multiplier = StartMultiplier;

        while (!stoppingToken.IsCancellationRequested)
        {
            multiplier = await ExecuteInternal(multiplier);
            await Task.Delay(multiplier * OneSecond, stoppingToken);
        }
    }

    private async Task<int> ExecuteInternal(int multiplier)
    {
        var umInfo = await GetUnderMaintenanceTime();

        if (umInfo.IsUnderMaintenanceTime)
        {
            await loggerService.Info(
                $"{scheduleJob.JobInfo} is Under Maintenance from {umInfo.From} to {umInfo.To}.");

            return umDelayTimeInSeconds;
        }

        var sw = Stopwatch.StartNew();

        try
        {
            var hasData = await ExecuteJob();
            var calculatedMultiplier = CalculateMultiplier(hasData, multiplier);

            if (scheduleJob.IsLogAfterExecuteJob)
            {
                sw.Stop();

                var logInfo = hasData
                    ? $"Total execution time {sw.ElapsedMilliseconds} milliseconds"
                    : "No Data to Import";
                await loggerService.Info($"{scheduleJob.JobInfo}: {logInfo}. SleepTimeInSeconds={calculatedMultiplier}");
            }

            return calculatedMultiplier;
        }
        catch (Exception ex)
        {
            await HandleException(ex);
        }

        return StartMultiplier;
    }

    private async Task<UnderMaintenanceTime> GetUnderMaintenanceTime()
    {
        try
        {
            return await maintenanceService.GetUnderMaintenanceTimeAsync();
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, scheduleJob, nameof(GetUnderMaintenanceTime), "Could not get UM information.");
        }

        return new UnderMaintenanceTime();
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

    private Task HandleException(Exception exception)
        => exception.Message.Contains("The account is disabled", StringComparison.OrdinalIgnoreCase)
        ? loggerService.Info($"{scheduleJob.JobInfo}: {exception.Message}")
        : loggerService.Error(exception, scheduleJob, nameof(ExecuteInternal));

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await loggerService.Info($"{scheduleJob.JobInfo} is stopping.");

        await base.StopAsync(cancellationToken);
    }

    protected abstract Task<bool> ExecuteJob();
}
