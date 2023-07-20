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
        private const int delayTimeToRunRetryJob = 2 * 60000;
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
            var dt = DateTime.Now;
            var currentTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            JobConfigModel jobConfigModel = new JobConfigModel();

            try
            {
                jobConfigModel = await transferService.GetJobConfigModel();
            }
            catch (Exception e)
            {
                await HandleException(e);
                await StopAsync(stoppingToken);
            }

            if (!jobConfigModel.IsActive)
            {
                return;
            }

            var runningTime = jobConfigModel.GetRunningTime(currentTime);
            var isRunTime = IsRunTime(runningTime, currentTime);

            if (!isRunTime)
            {
                return;
            }

            await Task.Delay(delayTimeToRunRetryJob, stoppingToken);

            jobConfigModel = await transferService.GetJobConfigModel();
            await loggerService.Info($"{scheduleJob.JobInfo} start transferring.");

            if (!jobConfigModel.HasCompletedToday(DateTime.Now))
            {
                while (true)
                {
                    try
                    {
                        await WaitForSystemUM(stoppingToken);

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
            }

            await ImportLogs();
            await loggerService.Info($"{scheduleJob.JobInfo} run completed.");
        }

        private Task ProcessBatch(BatchModel batch)
            => transferService.ProcessRetryBatch(batch);
    }
}