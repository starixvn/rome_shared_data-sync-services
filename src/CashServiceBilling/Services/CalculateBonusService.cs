using System.Text;
using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Services;

public interface ICalculateBonusService
{
    Task CalculateAndAddBonusTransactions(JobsSettingsModel jobsSettings, ScheduleJob scheduleJob);
}

public class CalculateBonusService : ICalculateBonusService
{
    private const string JobName = "[Calculate Bonus]";
    private readonly IDynamicRepository repository;
    private readonly ILoggerService logger;

    public CalculateBonusService(IDynamicRepository repository, ILoggerService logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task CalculateAndAddBonusTransactions(JobsSettingsModel jobsSettings, ScheduleJob scheduleJob)
    {
        await logger.Notify($"Start {JobName}.", jobsSettings.NotifyCategory);
        var agentPromotionsCount = 0;

        try
        {
            var agentPromotions = await GetAgentSettingBonus(jobsSettings.IsTest);
            agentPromotionsCount = agentPromotions?.Count() ?? 0;

            if (agentPromotionsCount > 0)
            {
                await CalculateCashPromotionsFlow(agentPromotions, scheduleJob);
                await AddBonusTransactionsFlow(jobsSettings.SiteDBGroupIds);
            }
        }
        catch (Exception ex)
        {
            await logger.Error(ex, scheduleJob, nameof(CalculateAndAddBonusTransactions));
        }

        await logger.Notify($"{JobName} AgentSettingBonus Count: {agentPromotionsCount}.", jobsSettings.NotifyCategory);
        await logger.Notify($"{JobName} completed.", jobsSettings.NotifyCategory);
    }

    public async Task<AgentPromotion[]> GetAgentSettingBonus(bool isTest)
    {
        var (mondayLastWeek, sundayLastWeek) = GetMondaySundayLastWeek();
        var criteria = new GetAgentSettingBonusCriteria(isTest, mondayLastWeek, sundayLastWeek);
        await repository.ExecuteAsync(criteria);

        return string.IsNullOrWhiteSpace(criteria.AgentPromotion)
            ? Array.Empty<AgentPromotion>()
            : JsonSerializer.Deserialize<AgentPromotion[]>(criteria.AgentPromotion);
    }

    public static (DateTime, DateTime) GetMondaySundayLastWeek()
    {
        var today = DateTime.Today;
        var daysSinceSunday = (int)today.DayOfWeek;
        var sundayLastWeek = today.AddDays(-daysSinceSunday);
        var mondayLastWeek = sundayLastWeek.AddDays(-6);

        return (mondayLastWeek, sundayLastWeek);
    }

    public async Task CalculateCashPromotionsFlow(AgentPromotion[] agentPromotions, ScheduleJob scheduleJob)
    {
        var productDetailsXmlTask = GetProductDetailsXml();
        var reportDateTask = GetBalanceUpDayAndMoveBetsDay();
        await Task.WhenAll(reportDateTask, productDetailsXmlTask);
        var productDetailsXml = await productDetailsXmlTask;
        var reportDate = await reportDateTask;
        var batchSize = GetBatchSize(scheduleJob);

        var tasks = agentPromotions.Chunk(batchSize).Select(p => CalculateCashPromotions(
            productDetailsXml,
            reportDate.MoveBetsDay,
            reportDate.BalanceUpDay,
            JsonSerializer.Serialize(p),
            scheduleJob
        ));
        await Task.WhenAll(tasks);
    }

    public async Task CalculateCashPromotions(
        string productDetailsXml,
        DateTime moveBetsDay,
        DateTime balanceUpDay,
        string agentPromotion,
        ScheduleJob scheduleJob)
    {
        try
        {
            await repository.ExecuteAsync(new CalculateCashPromotionCriteria(
                productDetailsXml,
                moveBetsDay,
                balanceUpDay,
                agentPromotion));
        }
        catch (Exception ex)
        {
            await logger.Error(ex, scheduleJob, nameof(CalculateCashPromotions));
        }
    }

    public static int GetBatchSize(ScheduleJob scheduleJob)
    {
        var batchSizeParameter = scheduleJob.Parameters
            .FirstOrDefault(p => p.Key.Equals("BatchSize", StringComparison.OrdinalIgnoreCase));

        return batchSizeParameter != null && int.TryParse(batchSizeParameter.Value, out var batchSize)
            ? batchSize
            : 3000; // Default batch size
    }

    public Task<BalanceUpDayAndMoveBetsDay> GetBalanceUpDayAndMoveBetsDay()
        => repository.GetAsync<BalanceUpDayAndMoveBetsDay>(new GetBalanceUpDayAndMoveBetsDayCriteria());

    public async Task<string> GetProductDetailsXml()
    {
        var xml = new StringBuilder();
        xml.Append("<ROOT>");

        var productDetails = await repository.FetchAsync<ProductDetail>(new GetProductsDetailCriteria());

        foreach (var item in productDetails)
        {
            xml.Append(item.ToXml());
        }

        xml.Append("</ROOT>");

        return xml.ToString();
    }

    public async Task AddBonusTransactionsFlow(IEnumerable<int> siteDBGroupIds)
    {
        foreach (var siteDBGroupId in siteDBGroupIds)
        {
            var maxCustId = 0;
            string memberBonus;

            while (true)
            {
                (maxCustId, memberBonus) = await GetBonusTransactions(siteDBGroupId, maxCustId);

                if (string.IsNullOrWhiteSpace(memberBonus))
                {
                    break;
                }

                await AddBonusTransactions(memberBonus, siteDBGroupId);
                await Task.Delay(100);
            }
        }
    }

    public async Task<(int, string)> GetBonusTransactions(int siteDBGroupId, int maxCustId)
    {
        var criteria = new GetBonusTransactionCriteria(siteDBGroupId, maxCustId);
        await repository.ExecuteAsync(criteria);

        return (criteria.MaxCustId, criteria.MemberBonus);
    }

    public Task AddBonusTransactions(string memberBonus, int siteDBGroupId)
        => repository.ExecuteAsync(new AddBonusTransactionCriteria(memberBonus, siteDBGroupId));
}
