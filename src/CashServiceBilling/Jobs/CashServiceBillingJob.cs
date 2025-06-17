using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using CashServiceBilling.Extensions;

namespace CashServiceBilling.Jobs
{
    public sealed class CashServiceBillingJob : BaseCashServiceBillingJob
    {
        public CashServiceBillingJob(
            ScheduleJob[] scheduleJobs,
            ILoggerService loggerService,
            IMaintenanceService maintenanceService,
            UMClientConfiguration umClientConfiguration,
            ICashServiceBillingService cashServiceBillingService,
            IOptions<JobsSettingsModel> settings)
            : base(
                scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(CashServiceBillingJob), StringComparison.OrdinalIgnoreCase)) ?? new ScheduleJob(),
                loggerService,
                maintenanceService,
                umClientConfiguration,
                cashServiceBillingService,
                settings)
        {
        }

        public override async Task ExecuteInternal(CancellationToken stoppingToken)
        {
            var jobConfig = cashServiceBillingService.GetJobConfig();
            var isRunningTime = jobConfig.IsRunTime();
            if (!isRunningTime)
            {
                await WaitingToNextExecution(jobConfig, stoppingToken);
                return;
            }

            await loggerService.Error(new Exception($"{scheduleJob.JobInfo} start cash service billing."), scheduleJob, nameof(ExecuteInternal));
            await Notify("Start cash service billing.");

            try
            {
                await WaitForSystemUM(stoppingToken);
                var batchList = await cashServiceBillingService.GetAgentList(settings.SiteDBGroupIds);
                foreach (var batch in batchList)
                {
                    await ProcessBatch(batch);
                }

                await Notify("Cash service billing completed.");
                await loggerService.Error(new Exception($"{scheduleJob.JobInfo} completed cash service billing."), scheduleJob, nameof(ExecuteInternal));
            }
            catch (Exception ex)
            {
                await Notify($"Cash service billing failed: {ex.Message}.");
                await loggerService.Error(ex, scheduleJob, nameof(ExecuteInternal));
            }

            await WaitingToNextExecution(jobConfig, stoppingToken);
        }

        private async Task WaitingToNextExecution(JobConfigModel jobConfig, CancellationToken stoppingToken)
        {
            var currentTime = DateTimeExtensions.Now;
            var executionTime = jobConfig.GetDefaultTime();
            var todayDefaultTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, executionTime.Hour, executionTime.Minute, 0);
            var nextExecutionTime = currentTime < todayDefaultTime ? todayDefaultTime : todayDefaultTime.AddDays(1);
            var delayTime = nextExecutionTime.Subtract(currentTime).TotalMilliseconds;

            await Notify($"{currentTime} waiting to next execution {nextExecutionTime} in {delayTime} milliseconds");
            await Task.Delay((int)delayTime, stoppingToken);
        }

        private async Task ProcessBatch(BatchModel batch)
        {
            try
            {
                var result = await cashServiceBillingService.StartJob(batch);
                if (result.ErrCode != 0)
                {
                    await Notify(result.ErrMsg);
                }
            }
            catch (Exception ex)
            {
                await Notify($"Cash service billing failed: {batch.SiteDBGroupId} {batch.AgentList} {ex.Message}.");
            }
        }
    }
}