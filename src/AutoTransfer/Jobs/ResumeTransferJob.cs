using AutoTransfer.Constants;
using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace AutoTransfer.Jobs
{
    public sealed class ResumeTransferJob : BaseAutoTransferJob
    {
        private const string runningMode = RunningModeConstant.Resume;

        public ResumeTransferJob(
            ScheduleJob[] scheduleJobs,
            ILoggerService loggerService,
            IMaintenanceService maintenanceService,
            UMClientConfiguration umClientConfiguration,
            ITransferService transferService,
            IOptions<JobsSettingsModel> settings)
            : base(
                scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(ResumeTransferJob), StringComparison.OrdinalIgnoreCase)) ?? new ScheduleJob(),
                loggerService,
                maintenanceService,
                umClientConfiguration,
                transferService,
                settings)
        {
        }

        public override async Task ExecuteInternal(CancellationToken stoppingToken)
        {
            await WaitingToSchedule(stoppingToken);

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            await WaitForSystemUM(stoppingToken);
            await WaitForBalanceUpProcess(stoppingToken);
            var startModel = await transferService.Start(settings.PairOfBatch, runningMode);

            while (startModel.TotalBatch > 0)
            {
                try
                {
                    await WaitForSystemUM(stoppingToken);
                    await WaitForBalanceUpProcess(stoppingToken);

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
                    await Task.Delay(settings.DelayRetryJobTimeInMilliSeconds, stoppingToken);
                }
            }

            loggerService.InfoConsole("{JobInfo} run completed.", scheduleJob.JobInfo);
            await StopAsync(stoppingToken);
        }

        private Task ProcessBatch(BatchModel batch)
            => transferService.ProcessBatch(batch);
    }
}