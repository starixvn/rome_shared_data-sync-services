using AutoTransfer.Constants;
using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace AutoTransfer.Jobs;

public sealed class ResumeTransferJob : BaseAutoTransferJob
{
    private readonly ITransferService transferService;
    private readonly JobsSettingsModel settings;
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

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

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
        => transferService.ProcessBatch(batch);
}
