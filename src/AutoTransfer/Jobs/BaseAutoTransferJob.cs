using System.Text.Json;
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

                await Task.Delay(settings.StartJobCheckingIntervalInMilliSeconds, stoppingToken);
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
                ToJson("No UM");
                break;
            }

            Console.WriteLine(ToJson(umInfo));

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
            await transferService.ImportLogs(settings.Environment);
        }
        catch (Exception ex)
        {
            await HandleException(ex);
        }
    }

    protected Task Notify(string message)
    {
        var siteDBName = string.IsNullOrWhiteSpace(settings.SiteDBName)
            ? string.Empty
            : $" - [{settings.SiteDBName}]";

        return loggerService.Notify($"{scheduleJob.JobInfo}{siteDBName} {message}", settings.NotifyCategory);
    }

    public static string ToJson(object value) => JsonSerializer.Serialize(value);
}
