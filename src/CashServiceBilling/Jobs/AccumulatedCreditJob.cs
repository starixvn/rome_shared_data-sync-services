using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using Shared.Configuration;

namespace CashServiceBilling.Jobs;

public sealed class AccumulatedCreditJob
{
    private readonly JobsSettingsModel jobsSettings;
    private readonly ScheduleJob scheduleJob;
    private readonly IAccumulatedCreditService accumulatedCreditService;

    public AccumulatedCreditJob(
        IOptions<JobsSettingsModel> jobsSettings,
        ScheduleJob[] scheduleJobs,
        IAccumulatedCreditService accumulatedCreditService)
    {
        this.jobsSettings = jobsSettings.Value;
        scheduleJob = scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(AccumulatedCreditJob), StringComparison.OrdinalIgnoreCase));
        this.accumulatedCreditService = accumulatedCreditService;
    }

    public string Cron => jobsSettings.AccumulatedCreditJobCron;

    public async Task ExecuteAsync()
    {
        if (scheduleJob == null || !scheduleJob.IsActive)
        {
            return;
        }

        var batchSizeParameter = scheduleJob.Parameters.FirstOrDefault(p => p.Key.Equals("BatchSize", StringComparison.OrdinalIgnoreCase));
        if (batchSizeParameter == null || !int.TryParse(batchSizeParameter.Value, out var batchSize))
        {
            batchSize = 500;
        }

        await accumulatedCreditService.AccumulateCreditToAgent(jobsSettings.IsTest, batchSize);
    }
}
