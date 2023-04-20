using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace AutoTransfer.Jobs
{
    public sealed class AutoTransferRetryJob : BaseAutoTransferJob
    {
        public AutoTransferRetryJob(
            ScheduleJob[] scheduleJobs,
            ILoggerService loggerService,
            IMaintenanceService maintenanceService,
            UMClientConfiguration umClientConfiguration,
            ITransferService transferService,
            IOptions<JobsSettingsModel> settings)
            : base(
                scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(AutoTransferRetryJob), StringComparison.OrdinalIgnoreCase)) ?? new ScheduleJob(),
                loggerService,
                maintenanceService,
                umClientConfiguration,
                transferService,
                settings)
        {
        }

        public override async Task ExecuteInternal(CancellationToken stoppingToken)
        {
            var isRunTime = IsRunTime();

            if (!isRunTime)
            {
                await WaitingToNextSchedule(stoppingToken);
            }

            while (true)
            {
                try
                {
                    await WaitForSystemUM(stoppingToken);
                    await WaitForBalanceUpProcess(stoppingToken);

                    var retryInfo = await transferService.SelectRetryBatches(settings.NumberOfThreads);

                    if (retryInfo.IsCompleted)
                    {
                        break;
                    }

                    if (!retryInfo.Batches.Any())
                    {
                        await Task.Delay(settings.DelayRetryJobTimeInMilliSeconds, stoppingToken);
                        continue;
                    }

                    var threads = new List<Task>();

                    foreach (BatchModel batch in retryInfo.Batches)
                    {
                        threads.Add(ProcessBatch(batch));
                    }

                    await Task.WhenAll(threads);
                }
                catch (Exception ex)
                {
                    await HandleException(ex);
                    await Task.Delay(settings.DelayRetryJobTimeInMilliSeconds, stoppingToken);
                }
            }

            await ImportLogs();
            loggerService.InfoConsole("{JobInfo} run completed.", scheduleJob.JobInfo);
            await Task.Delay(settings.DelayRetryJobTimeInMilliSeconds, stoppingToken);
        }

        private Task ProcessBatch(BatchModel batch)
            => transferService.ProcessRetryBatch(batch);
    }
}