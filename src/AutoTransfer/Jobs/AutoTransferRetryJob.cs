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
        private readonly JobsSettingsModel settings;
        private readonly ITransferService transferService;

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
                umClientConfiguration)
        {
            this.settings = settings.Value;
            this.transferService = transferService;
        }

        public override async Task ExecuteInternal(CancellationToken stoppingToken)
        {
            var isRunTime = IsRunTime();

            if (!isRunTime)
            {
                await WaitingToNextSchedule(stoppingToken);
            }

            bool isComplete = false;

            while (!isComplete)
            {
                try
                {
                    RetryBatchesModel retryInfo = await transferService.SelectRetryBatches(settings.NumberOfThreads);

                    if (retryInfo.IsCompleted)
                    {
                        break;
                    }

                    List<Task> threads = new List<Task>();

                    foreach (BatchModel batch in retryInfo.Batches)
                    {
                        threads.Add(ProcessBatch(batch));
                    }

                    if (threads.Count == 0)
                    {
                        break;
                    }

                    await Task.WhenAll(threads);
                    await Task.Delay(settings.DelayRetryJobTimeInMilliSeconds, stoppingToken);
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

        private async Task ImportLogs()
        {
            try
            {
                await transferService.ImportLogs();
            }
            catch (Exception ex)
            {
                await HandleException(ex);
            }
        }
    }
}