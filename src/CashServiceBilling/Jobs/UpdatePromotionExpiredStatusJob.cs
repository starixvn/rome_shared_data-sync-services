using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using Shared.Configuration;

namespace CashServiceBilling.Jobs;

public sealed class UpdatePromotionExpiredStatusJob
{
    private readonly JobsSettingsModel jobsSettings;
    private readonly ScheduleJob scheduleJob;
    private readonly IPromotionService promotionService;

    public UpdatePromotionExpiredStatusJob(
        IOptions<JobsSettingsModel> jobsSettings,
        ScheduleJob[] scheduleJobs,
        IPromotionService promotionService)
    {
        this.jobsSettings = jobsSettings.Value;
        scheduleJob = scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(UpdatePromotionExpiredStatusJob), StringComparison.OrdinalIgnoreCase));
        this.promotionService = promotionService;
    }

    public string Cron => jobsSettings.UpdatePromotionExpiredStatusJobCron;

    public async Task ExecuteAsync()
    {
        if (scheduleJob == null || !scheduleJob.IsActive)
        {
            return;
        }

        await promotionService.UpdateExpiredStatus(jobsSettings, scheduleJob);
    }
}