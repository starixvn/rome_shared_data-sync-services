using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;
using ResetPotentialWinLimit.Models;
using ResetPotentialWinLimit.Services;

namespace ResetPotentialWinLimit.Jobs
{
    public sealed class ResetPotentialWinLimitJob : BaseResetPotentialWinLimitJob
    {
        private const int MAX_RETRY_COUNT = 5;

        public ResetPotentialWinLimitJob(
            ScheduleJob[] scheduleJobs,
            ILoggerService loggerService,
            IMaintenanceService maintenanceService,
            UMClientConfiguration umClientConfiguration,
            IResetPotentialWinLimitService resetPotentialWinLimitService,
            IOptions<JobsSettingsModel> settings)
            : base(
                scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(ResetPotentialWinLimitJob), StringComparison.OrdinalIgnoreCase)) ?? new ScheduleJob(),
                loggerService,
                maintenanceService,
                umClientConfiguration,
                resetPotentialWinLimitService,
                settings)
        {
        }

        public override async Task ExecuteInternal(CancellationToken stoppingToken)
        {
            var dt = DateTime.Now;
            var currentTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            var jobConfigModel = resetPotentialWinLimitService.GetJobConfigModel(settings.Environment);

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

            await WaitForSystemUM(stoppingToken);
            await resetPotentialWinLimitService.Start();

            await loggerService.Error(new Exception($"{scheduleJob.JobInfo} start resetting potential win limit."), scheduleJob, nameof(ExecuteInternal));
            await Notify("Start resetting potential win limit.");

            while (true)
            {
                try
                {
                    await WaitForSystemUM(stoppingToken);

                    var superIds = await resetPotentialWinLimitService.SelectSuperList(settings.BatchSize);
                    if (!superIds.Any())
                    {
                        break;
                    }

                    var threads = new List<Task>();
                    foreach (var superId in superIds)
                    {
                        var memberIds = await resetPotentialWinLimitService.SelectMemberList(superId);
                        threads.Add(ProcessBatch(superId, memberIds, stoppingToken));
                        await Task.WhenAll(threads);
                    }
                }
                catch (Exception ex)
                {
                    await HandleException(ex);
                    await Task.Delay(settings.DelayExecuteBatchesTimeInMilliSeconds, stoppingToken);
                }
            }

            await Notify("completed resetting potential win limit.");
            await loggerService.Error(new Exception($"{scheduleJob.JobInfo} completed resetting potential win limit."), scheduleJob, nameof(ExecuteInternal));
        }

        private async Task ProcessBatch(int superId, IEnumerable<int> memberIds, CancellationToken stoppingToken)
        {
            var retryCount = 0;
            var custList = string.Join(",", memberIds);

            while (retryCount < MAX_RETRY_COUNT)
            {
                var result = await resetPotentialWinLimitService.ProcessBatch(custList);

                if (result.ErrCode == 0)
                {
                    break;
                }

                await Task.Delay(settings.DelayRetryInMilliSeconds, stoppingToken);
                retryCount++;
            }

            var isCompleted = retryCount < MAX_RETRY_COUNT;
            await resetPotentialWinLimitService.UpdateMemberStatus(custList, isCompleted);
            await resetPotentialWinLimitService.UpdateSuperStatus(superId, isCompleted);
        }
    }
}