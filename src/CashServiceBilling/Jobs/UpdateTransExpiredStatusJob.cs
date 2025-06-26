using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Jobs;

public sealed class UpdateTransExpiredStatusJob : BaseCashServiceBillingJob
{
    private readonly ITransactionService transactionService;

    public UpdateTransExpiredStatusJob(
        ScheduleJob[] scheduleJobs,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        ITransactionService transactionService,
        IOptions<JobsSettingsModel> settings) : base(
            scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(UpdateTransExpiredStatusJob), StringComparison.OrdinalIgnoreCase)) ?? new ScheduleJob(),
            loggerService,
            maintenanceService,
            umClientConfiguration,
            settings)
    {
        this.transactionService = transactionService;
    }

    public override async Task ExecuteInternal(CancellationToken stoppingToken)
    {
        try
        {
            await WaitForSystemUM(stoppingToken);
            await transactionService.UpdateExpiredStatus(settings.IsTest);
        }
        catch (Exception ex)
        {
            await HandleException(ex);
        }

        await Task.Delay(TimeSpan.FromSeconds(scheduleJob.DelayExecutionTimeInSeconds), stoppingToken);
    }
}