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
    private readonly ITransactionLogService transactionLogService;

    public CalculateBonusService(
        IDynamicRepository repository,
        ILoggerService logger,
        ITransactionLogService transactionLogService)
    {
        this.repository = repository;
        this.logger = logger;
        this.transactionLogService = transactionLogService;
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
                await AddBonusTransactionsFlow(jobsSettings);
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

    public static DateTime GetTuesdayLastWeek()
    {
        var today = DateTime.Today;
        var daysToSubtract = (int)today.DayOfWeek - (int)DayOfWeek.Tuesday;

        return daysToSubtract == 0
            ? today
            : today.AddDays(-daysToSubtract);
    }

    public async Task CalculateCashPromotionsFlow(AgentPromotion[] agentPromotions, ScheduleJob scheduleJob)
    {
        var productDetailsXmlTask = GetProductDetailsXml();
        var reportDateTask = GetBalanceUpDayAndMoveBetsDay();
        await Task.WhenAll(reportDateTask, productDetailsXmlTask);
        var productDetailsXml = await productDetailsXmlTask;
        var reportDate = await reportDateTask;
        var batchSize = GetBatchSize(scheduleJob);
        var transDate = GetTuesdayLastWeek();

        var agentPromotionsInBatch = agentPromotions.Chunk(batchSize);
        foreach (var item in agentPromotionsInBatch)
        {
            await CalculateCashPromotions(
                productDetailsXml,
                reportDate.MoveBetsDay,
                reportDate.BalanceUpDay,
                JsonSerializer.Serialize(item),
                transDate,
                scheduleJob);
        }
    }

    public async Task CalculateCashPromotions(
        string productDetailsXml,
        DateTime moveBetsDay,
        DateTime balanceUpDay,
        string agentPromotion,
        DateTime transDate,
        ScheduleJob scheduleJob)
    {
        try
        {
            await repository.ExecuteAsync(new CalculateCashPromotionCriteria(
                productDetailsXml,
                moveBetsDay,
                balanceUpDay,
                agentPromotion,
                transDate));
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

    public async Task AddBonusTransactionsFlow(JobsSettingsModel jobsSettings)
    {
        var hasAnyBonusChanged = false;
        var transDate = GetTuesdayLastWeek();

        foreach (var siteDBGroupId in jobsSettings.SiteDBGroupIds)
        {
            var maxCustId = 0;
            string memberBonus;

            while (true)
            {
                (maxCustId, memberBonus) = await GetBonusTransactions(siteDBGroupId, maxCustId, transDate);

                if (string.IsNullOrWhiteSpace(memberBonus))
                {
                    break;
                }

                var isBonusChanged = await AddBonusTransactionsAndWriteLog(memberBonus, siteDBGroupId, transDate);
                if (isBonusChanged)
                {
                    hasAnyBonusChanged = true;
                }

                await Task.Delay(100);
            }
        }

        if (hasAnyBonusChanged)
        {
            await logger.Notify("[ALERT] Bonus transaction amount mismatch detected (same CustId, TransactionTypeID, Date). Logged to CashTransactionError for review.", jobsSettings.NotifyCategory);
        }
    }

    public async Task<(int, string)> GetBonusTransactions(int siteDBGroupId, int maxCustId, DateTime transDate)
    {
        var criteria = new GetBonusTransactionCriteria(siteDBGroupId, maxCustId, transDate);
        await repository.ExecuteAsync(criteria);

        return (criteria.MaxCustId, criteria.MemberBonus);
    }

    public async Task<bool> AddBonusTransactionsAndWriteLog(string memberBonus, int siteDBGroupId, DateTime transDate)
    {
        var criteria = new AddBonusTransactionCriteria(memberBonus, siteDBGroupId, transDate);
        await repository.ExecuteAsync(criteria);

        await transactionLogService.WriteLog(criteria.LogJson, (short)siteDBGroupId);

        return criteria.IsBonusChanged;
    }
}
