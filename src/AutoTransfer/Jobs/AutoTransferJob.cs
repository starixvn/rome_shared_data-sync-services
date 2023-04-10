using AutoTransfer.Constants;
using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace AutoTransfer.Jobs;

public sealed class AutoTransferJob : BaseAutoTransferJob
{
    private const string runningMode = RunningModeConstant.Normal;

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
        var isRunTime = IsRunTime();

        if (!isRunTime)
        {
            await WaitingToNextSchedule(stoppingToken);
        }

        await WaitForSystemUM(stoppingToken);
        await WaitForBalanceUpProcess(stoppingToken);
        var startModel = await transferService.Start(settings.PairOfBatch, runningMode);
        var isComplete = startModel.TotalBatch == 0;

        while (!isComplete)
        {
            try
            {
                var batches = await transferService.SelectBatches(settings.NumberOfThreads, runningMode);
                var threads = new List<Task>();

                foreach (var batch in batches)
                {
                    threads.Add(ProcessBatch(batch));
                }

                if (threads.Count == 0)
                {
                    break;
                }

                await Task.WhenAll(threads);
                await Task.Delay(settings.DelayExecuteBatchesTime, stoppingToken);
            }
            catch (Exception ex)
            {
                await HandleException(ex);
                await Task.Delay(settings.DelayRetryJobTimeInMilliSeconds, stoppingToken);
            }
        }

        loggerService.InfoConsole("{JobInfo} run completed.", scheduleJob.JobInfo);
        await Task.Delay(settings.DelayRetryJobTimeInMilliSeconds, stoppingToken);
    }

    private Task ProcessBatch(BatchModel batch)
        => transferService.ProcessBatch(batch);
}
