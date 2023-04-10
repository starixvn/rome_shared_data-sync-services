using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace AutoTransfer.Jobs
{
    public sealed class ResumeTransferRetryJob : BaseAutoTransferJob
    {
        public ResumeTransferRetryJob(
            ScheduleJob[] scheduleJobs,
            ILoggerService loggerService,
            IMaintenanceService maintenanceService,
            UMClientConfiguration umClientConfiguration,
            ITransferService transferService,
            IOptions<JobsSettingsModel> settings)
            : base(
                scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(ResumeTransferRetryJob), StringComparison.OrdinalIgnoreCase)) ?? new ScheduleJob(),
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

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            await WaitForSystemUM(stoppingToken);
            await WaitForBalanceUpProcess(stoppingToken);
            bool isComplete = false;

            while (!isComplete)
            {
                try
                {
                    var retryInfo = await transferService.SelectRetryBatches(settings.NumberOfThreads);

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
            await StopAsync(stoppingToken);
        }

        public async override Task WaitingToNextSchedule(CancellationToken stoppingToken)
        {
            var currentTime = DateTime.Now;
            var executionTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, scheduleJob.DelayExecutionAtExactlyTime.Value.Hour, scheduleJob.DelayExecutionAtExactlyTime.Value.Minute, 0);

            var todayExecutionTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, executionTime.Hour, executionTime.Minute, 0);
            if (todayExecutionTime < currentTime)
            {
                await StopAsync(stoppingToken);
                return;
            }

            var timeDelay = todayExecutionTime - currentTime;
            loggerService.InfoConsole("{JobInfo} delay to {next}", scheduleJob.JobInfo, timeDelay);
            await Task.Delay(timeDelay, stoppingToken);
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