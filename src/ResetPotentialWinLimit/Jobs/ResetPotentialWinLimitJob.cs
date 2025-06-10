using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;
using ResetPotentialWinLimit.Models;
using ResetPotentialWinLimit.Services;
using ResetPotentialWinLimit.Extensions;

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
            var jobConfig = await resetPotentialWinLimitService.GetJobConfig();
            var jobStatus = await resetPotentialWinLimitService.GetJobStatus();

            if (jobStatus.RunningDate == null || !DateTimeExtentions.IsSameDate(jobStatus.RunningDate.Value.Date, DateTimeExtentions.Now.Date))
            {
                jobStatus.LastMaxCustID = 0;
                jobStatus.CompletedTime = null;
            }

            if (!jobConfig.IsActive && jobStatus.HasCompletedToday())
            {
                await WaitingToNextExecution(jobConfig, jobStatus, stoppingToken);
                return;
            }

            var isRunningTime = jobConfig.IsRunTime(jobStatus);

            if (!isRunningTime)
            {
                await WaitingToNextExecution(jobConfig, jobStatus, stoppingToken);
                return;
            }

            await loggerService.Error(new Exception($"{scheduleJob.JobInfo} start resetting potential win limit."), scheduleJob, nameof(ExecuteInternal));
            await Notify("Start resetting potential win limit.");

            jobStatus.RunningDate = DateTimeExtentions.Now;
            var isCompleted = false;

            while (true)
            {
                try
                {
                    await WaitForSystemUM(stoppingToken);
                    var memberBatch = await resetPotentialWinLimitService.SelectMemberList(settings.BatchSize, jobStatus.LastMaxCustID);
                    if (!memberBatch.MemberIds.Any())
                    {
                        jobStatus.CompletedTime = DateTimeExtentions.Now;
                        await resetPotentialWinLimitService.UpdateJobStatus(jobStatus);
                        isCompleted = true;
                        break;
                    }

                    var processResult = await ProcessBatch(jobStatus, memberBatch, stoppingToken);
                    if (processResult.ErrCode != 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    await HandleException(ex);
                    break;
                }
            }

            if (isCompleted)
            {
                await Notify("completed resetting potential win limit.");
                await loggerService.Error(new Exception($"{scheduleJob.JobInfo} completed resetting potential win limit."), scheduleJob, nameof(ExecuteInternal));
                await WaitingToNextExecution(jobConfig, jobStatus, stoppingToken);
            }
            else
            {
                await Notify("failed resetting potential win limit.");
                await loggerService.Error(new Exception($"{scheduleJob.JobInfo} failed resetting potential win limit."), scheduleJob, nameof(ExecuteInternal));
                await StopAsync(stoppingToken);
            }
        }

        private async Task WaitingToNextExecution(
            JobConfigModel jobConfig,
            JobStatusModel jobStatus,
            CancellationToken stoppingToken)
        {
            var currentTime = DateTimeExtentions.Now;
            var executionTime = jobConfig.GetRunTime(jobStatus);
            var todayDefaultTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, executionTime.Hour, executionTime.Minute, 0);

            var defaultTime = jobConfig.GetDefaultTime();
            var nextDefaultTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, defaultTime.Hour, defaultTime.Minute, 0).AddDays(1);

            var nextExecutionTime = currentTime < todayDefaultTime ? todayDefaultTime : nextDefaultTime;
            var delayTime = nextExecutionTime.Subtract(currentTime).TotalMilliseconds;

            await Notify($"{currentTime} waiting to next execution {nextExecutionTime} in {delayTime} milliseconds");
            await Task.Delay((int)delayTime, stoppingToken);
        }

        private async Task<ResultModel> ProcessBatch(
            JobStatusModel jobStatus,
            MemberBatchModel memberBatch,
            CancellationToken stoppingToken)
        {
            var retryCount = 0;
            var custList = string.Join(",", memberBatch.MemberIds);

            while (retryCount < MAX_RETRY_COUNT)
            {
                try
                {
                    var processResult = await resetPotentialWinLimitService.ProcessBatch(custList);
                    if (processResult.ErrCode == 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    await HandleException(ex);
                }

                await Task.Delay(settings.DelayRetryInMilliSeconds, stoppingToken);
                retryCount++;
            }

            if (retryCount < MAX_RETRY_COUNT)
            {
                jobStatus.LastMaxCustID = memberBatch.MemberIds.Max();
                await resetPotentialWinLimitService.UpdateJobStatus(jobStatus);
                return new ResultModel { ErrCode = 0 };
            }

            await Notify($"Reset potential win limit failed for: {custList}");
            return new ResultModel { ErrCode = 1 };
        }
    }
}