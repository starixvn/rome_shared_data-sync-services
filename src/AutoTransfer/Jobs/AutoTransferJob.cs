using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace AutoTransfer.Jobs
{
    public sealed class AutoTransferJob : BaseAutoTransferJob
    {
        public AutoTransferJob(
            ScheduleJob[] scheduleJobs,
            ILoggerService loggerService,
            IMaintenanceService maintenanceService,
            UMClientConfiguration umClientConfiguration,
            ITransferService transferService,
            IOptions<JobsSettingsModel> settings)
            : base(
                scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(AutoTransferJob), StringComparison.OrdinalIgnoreCase)) ?? new ScheduleJob(),
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
            var jobConfigModel = await transferService.GetJobConfigModel();

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

            await loggerService.Error(new Exception($"{scheduleJob.JobInfo} start transferring."), scheduleJob, nameof(ExecuteInternal));

            await WaitForSystemUM(stoppingToken);
            var runningMode = jobConfigModel.GetRunningMode(currentTime);
            var startModel = await transferService.Start(settings.PairOfBatch, runningMode);

            while (startModel.TotalBatch > 0)
            {
                try
                {
                    await WaitForSystemUM(stoppingToken);

                    var batches = await transferService.SelectBatches(settings.NumberOfThreads, runningMode);
                    if (!batches.Any())
                    {
                        break;
                    }

                    var threads = new List<Task>();

                    foreach (BatchModel batch in batches)
                    {
                        threads.Add(ProcessBatch(batch));
                    }

                    await Task.WhenAll(threads);
                }
                catch (Exception ex)
                {
                    await HandleException(ex);
                    await Task.Delay(settings.DelayExecuteBatchesTimeInMilliSeconds, stoppingToken);
                }
            }

            await loggerService.Error(new Exception($"{scheduleJob.JobInfo} run completed."), scheduleJob, nameof(ExecuteInternal));
        }

        private Task ProcessBatch(BatchModel batch)
            => transferService.ProcessBatch(batch);
    }
}