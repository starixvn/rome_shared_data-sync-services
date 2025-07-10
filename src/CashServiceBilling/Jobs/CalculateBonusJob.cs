using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using Shared.Configuration;

namespace CashServiceBilling.Jobs;

public sealed class CalculateBonusJob
{
    private readonly JobsSettingsModel jobsSettings;
    private readonly ScheduleJob scheduleJob;
    private readonly ICalculateBonusService calculateBonusService;

    public CalculateBonusJob(
        IOptions<JobsSettingsModel> jobsSettings,
        ScheduleJob[] scheduleJobs,
        ICalculateBonusService calculateBonusService)
    {
        this.jobsSettings = jobsSettings.Value;
        scheduleJob = scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(CalculateBonusJob), StringComparison.OrdinalIgnoreCase));
        this.calculateBonusService = calculateBonusService;
    }

    public string Cron => jobsSettings.CalculateBonusJobCron;

    public async Task ExecuteAsync()
    {
        if (scheduleJob == null || !scheduleJob.IsActive)
        {
            return;
        }

        await calculateBonusService.AddBonusTransaction(jobsSettings.IsTest, scheduleJob);
    }
}