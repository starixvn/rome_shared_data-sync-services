using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Services;

public interface ICalculateBonusService
{
    Task AddBonusTransaction(bool isTest, ScheduleJob scheduleJob);
}

public class CalculateBonusService : ICalculateBonusService
{
    private const string JobName = "[Calculate Bonus]";
    private readonly IDynamicRepository repository;
    private readonly ILoggerService logger;
    private readonly IConfiguration configuration;

    public CalculateBonusService(
        IDynamicRepository repository,
        ILoggerService logger,
        IConfiguration configuration)
    {
        this.repository = repository;
        this.logger = logger;
        this.configuration = configuration;
    }

    public async Task AddBonusTransaction(bool isTest, ScheduleJob scheduleJob)
    {
        var category = configuration.GetValue<string>("JobsSettings:NotifyCategory");
        await logger.Notify($"Start {JobName}.", category);

        var agentPromotionsCount = 0;
        var agentPromotionJson = await GetAgentPromotionJson(isTest);

        if (!string.IsNullOrWhiteSpace(agentPromotionJson))
        {
            var agentPromotions = JsonSerializer.Deserialize<AgentPromotion[]>(agentPromotionJson);
            agentPromotionsCount = agentPromotions?.Length ?? 0;

            if (agentPromotionsCount > 0)
            {
                var tasks = agentPromotions.GroupBy(
                    p => p.SiteDBGroupId,
                    (key, group) => AddBonusTransaction(group, key, scheduleJob));
                await Task.WhenAll(tasks);
            }
        }

        await logger.Notify($"{JobName} CashPromotions Count: {agentPromotionsCount}.", category);
        await logger.Notify($"{JobName} completed.", category);
    }

    public async Task<string> GetAgentPromotionJson(bool isTest)
    {
        var criteria = new GetAgentSettingBonusCriteria(isTest);
        await repository.ExecuteAsync(criteria);

        return criteria.AgentPromotion;
    }

    public Task AddBonusTransaction(IEnumerable<AgentPromotion> agentPromotions, short siteDBGroupId, ScheduleJob scheduleJob)
    {
        var batchSize = GetBatchSize(scheduleJob);
        var agentPromotionBatches = agentPromotions.Chunk(batchSize);
        return Task.WhenAll(agentPromotionBatches.Select(async batch =>
        {
            try
            {
                await repository.ExecuteAsync(new AddBonusTransactionCriteria(
                    JsonSerializer.Serialize(batch),
                    siteDBGroupId));
            }
            catch (Exception ex)
            {
                await logger.Error(ex, scheduleJob, nameof(AddBonusTransaction));
            }
        }));
    }

    private static int GetBatchSize(ScheduleJob scheduleJob)
    {
        var batchSizeParameter = scheduleJob.Parameters
            .FirstOrDefault(p => p.Key.Equals("BatchSize", StringComparison.OrdinalIgnoreCase));

        return batchSizeParameter != null && int.TryParse(batchSizeParameter.Value, out var batchSize)
            ? batchSize
            : 3000; // Default batch size
    }
}
