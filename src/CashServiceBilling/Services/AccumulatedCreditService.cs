using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Services;

public interface IAccumulatedCreditService
{
    public Task AccumulateCreditToAgent(JobsSettingsModel jobsSettings, ScheduleJob scheduleJob);
}

public class AccumulatedCreditService : IAccumulatedCreditService
{
    private const string JobName = "[Returning accumulated credit to Agent]";
    private readonly IDynamicRepository repository;
    private readonly ILoggerService logger;
    private readonly ITransactionLogService transactionLogService;

    public AccumulatedCreditService(
        IDynamicRepository repository,
        ILoggerService logger,
        ITransactionLogService transactionLogService)
    {
        this.repository = repository;
        this.logger = logger;
        this.transactionLogService = transactionLogService;
    }

    public async Task AccumulateCreditToAgent(JobsSettingsModel jobsSettings, ScheduleJob scheduleJob)
    {
        await logger.Notify($"Start {JobName}.", jobsSettings.NotifyCategory);

        var winlossCount = 0;
        try
        {
            var winlossJson = await GetMemberYesterdayWinLoss(jobsSettings.IsTest);

            if (!string.IsNullOrWhiteSpace(winlossJson))
            {
                var winloss = JsonSerializer.Deserialize<MemberYesterdayWL[]>(winlossJson);
                winlossCount = winloss?.Length ?? 0;

                if (winlossCount > 0)
                {
                    var winlossBatches = winloss.Chunk(GetBatchSize(scheduleJob));
                    await Task.WhenAll(winlossBatches.Select(ProcessMemberYesterdayWinLoss));
                }
            }
        }
        catch (Exception ex)
        {
            await logger.Error(ex, scheduleJob, nameof(AccumulateCreditToAgent));
        }

        await logger.Notify($"{JobName} Cash Member Yesterday WinLoss Count: {winlossCount}.", jobsSettings.NotifyCategory);
        await logger.Notify($"{JobName} completed.", jobsSettings.NotifyCategory);
    }

    public async Task<string> GetMemberYesterdayWinLoss(bool isTest)
    {
        var criteria = new GetCashMemberListCriteria(isTest);
        await repository.ExecuteAsync(criteria);

        return criteria.CustIdList;
    }

    public async Task ProcessMemberYesterdayWinLoss(IEnumerable<MemberYesterdayWL> memberYesterdayWL)
    {
        var (cashAccumCreditTblJson, logs) = await AddManualDepositTrans(JsonSerializer.Serialize(memberYesterdayWL));

        if (!string.IsNullOrWhiteSpace(cashAccumCreditTblJson))
        {
            var cashAccumCreditTbl = JsonSerializer.Deserialize<CashAccumCredit[]>(cashAccumCreditTblJson);
            var syncCashAccumCreditTasks = cashAccumCreditTbl.GroupBy(
                c => c.CustDBGroupId,
                (key, group) => SyncCashAccumCredit(JsonSerializer.Serialize(group), key));
            await Task.WhenAll(syncCashAccumCreditTasks);
        }

        if (logs.Any())
        {
            await transactionLogService.WriteLogs(logs);
        }
    }

    public async Task<Tuple<string, IEnumerable<LogEntry>>> AddManualDepositTrans(string custIdList)
    {
        var criteria = new AddManualDepositTransCriteria(custIdList);
        var logs = await repository.FetchAsync<LogEntry>(criteria);

        return Tuple.Create(criteria.CashAccumCreditTbl, logs);
    }

    public async Task<ResultModel> SyncCashAccumCredit(string cashAccumCreditTbl, short custDBGroupId)
    {
        var criteria = new SyncCashAccumCreditCriteria(cashAccumCreditTbl, custDBGroupId);
        await repository.ExecuteAsync(criteria);
        var result = new ResultModel { ErrCode = criteria.ErrCode, ErrMsg = criteria.ErrMsg };

        if (criteria.ErrCode != 0 && !string.IsNullOrWhiteSpace(criteria.ErrMsg))
        {
            await logger.Error($"{criteria.GetSettingKey()} failed. CashAccumCreditTbl: {cashAccumCreditTbl}, ErrCode: {criteria.ErrCode}, ErrMsg: {criteria.ErrMsg}");
        }

        return result;
    }

    public static int GetBatchSize(ScheduleJob scheduleJob)
    {
        var batchSizeParameter = scheduleJob.Parameters
            .FirstOrDefault(p => p.Key.Equals("BatchSize", StringComparison.OrdinalIgnoreCase));

        return batchSizeParameter != null && int.TryParse(batchSizeParameter.Value, out var batchSize)
            ? batchSize
            : 500; // Default batch size
    }
}

