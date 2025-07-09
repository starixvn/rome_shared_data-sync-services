using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;

namespace CashServiceBilling.Services;

public interface ICalculateBonusService
{
    Task AddBonusTransaction(bool isTest, int size);
}

public class CalculateBonusService : ICalculateBonusService
{
    private const string JobName = "[Calculate Bonus]";
    private readonly IDynamicRepository repository;
    private readonly Fanex.Logging.ILogger logger;
    private readonly IConfiguration configuration;

    public CalculateBonusService(
        IDynamicRepository repository,
        Fanex.Logging.ILogger logger,
        IConfiguration configuration)
    {
        this.repository = repository;
        this.logger = logger;
        this.configuration = configuration;
    }

    public async Task AddBonusTransaction(bool isTest, int size)
    {
        var category = configuration.GetValue<string>("JobsSettings:NotifyCategory");
        await logger.ErrorAsync($"Start {JobName}.", category);

        var agentPromotionCount = 0;
        var agentPromotionJson = await GetAgentSettingPromotion(isTest);

        if (!string.IsNullOrWhiteSpace(agentPromotionJson))
        {
            var agentPromotion = JsonSerializer.Deserialize<AgentSettingPromotion[]>(agentPromotionJson);
            agentPromotionCount = agentPromotion?.Length ?? 0;

            if (agentPromotionCount > 0)
            {
                var tasks = agentPromotion.GroupBy(
                    p => p.SiteDBGroupId,
                    (key, group) => AddBonusTransaction(group, key, size));
                await Task.WhenAll(tasks);
            }
        }

        await logger.ErrorAsync($"{JobName} Agent Setting Promotion Count: {agentPromotionCount}.", category);
        await logger.ErrorAsync($"{JobName} completed.", category);
    }

    public async Task<string> GetAgentSettingPromotion(bool isTest)
    {
        var criteria = new GetAgentSettingBonusCriteria(isTest);
        await repository.ExecuteAsync(criteria);

        return criteria.AgentPromotion;
    }

    public Task AddBonusTransaction(IEnumerable<AgentSettingPromotion> agentPromotion, short siteDBGroupId, int size)
    {
        var agentPromotionBatches = agentPromotion.Chunk(size);
        return Task.WhenAll(agentPromotionBatches.Select(batch => AddBonusTransaction(batch, siteDBGroupId)));
    }

    public Task AddBonusTransaction(IEnumerable<AgentSettingPromotion> agentPromotion, short siteDBGroupId)
        => repository.ExecuteAsync(new AddBonusTransactionCriteria(
            JsonSerializer.Serialize(agentPromotion),
            siteDBGroupId));
}
