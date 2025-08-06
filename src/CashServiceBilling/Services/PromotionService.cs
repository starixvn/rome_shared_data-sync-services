using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Services;

public interface IPromotionService
{
    Task UpdateExpiredStatus(JobsSettingsModel jobsSettings, ScheduleJob scheduleJob);
}

public class PromotionService : IPromotionService
{
    private const string JobName = "[Update Promotion Expired Status]";
    private readonly IDynamicRepository repository;
    private readonly ITransactionLogService transactionLogService;
    private readonly ILoggerService logger;

    public PromotionService(
        IDynamicRepository repository,
        ITransactionLogService transactionLogService,
        ILoggerService logger)
    {
        this.repository = repository;
        this.transactionLogService = transactionLogService;
        this.logger = logger;
    }

    public async Task UpdateExpiredStatus(JobsSettingsModel jobsSettings, ScheduleJob scheduleJob)
    {
        await logger.Notify($"Start {JobName}.", jobsSettings.NotifyCategory);

        try
        {
            var logs = await repository.FetchAsync<LogEntry>(
                new UpdatePromotionExpiredStatusCriteria(jobsSettings.IsTest));

            if (logs.Any())
            {
                await transactionLogService.WriteLogs(logs);
            }
        }
        catch (Exception ex)
        {
            await logger.Error(ex, scheduleJob, nameof(UpdateExpiredStatus));
        }

        await logger.Notify($"{JobName} completed.", jobsSettings.NotifyCategory);
    }
}