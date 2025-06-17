using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;
using CashServiceBilling.Models;
using CashServiceBilling.Services;

namespace CashServiceBilling.Jobs;

public class BaseCashServiceBillingJob : BackgroundServiceBase
{
    public readonly JobsSettingsModel settings;
    public readonly ICashServiceBillingService cashServiceBillingService;
    private const int UMDelayCheckingTimeInMilliseconds = 60000;

    public BaseCashServiceBillingJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        ICashServiceBillingService cashServiceBillingService,
        IOptions<JobsSettingsModel> settings)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
        this.settings = settings.Value;
        this.cashServiceBillingService = cashServiceBillingService;
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

            if (umInfo == null || !umInfo.IsUnderMaintenanceTime)
            {
                break;
            }

            await Task.Delay(UMDelayCheckingTimeInMilliseconds, stoppingToken);
        }
    }

    public Task Notify(string message)
    {
        return loggerService.Notify($"{scheduleJob.JobInfo} {message}", settings.NotifyCategory);
    }
}
