using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;

namespace CashServiceBilling.Services;

public interface IAccumulatedCreditService
{
    public Task AccumulateCreditToAgent(bool isTest, int size);
}

public class AccumulatedCreditService : IAccumulatedCreditService
{
    private const string JobName = "[Returning accumulated credit to Agent]";
    private readonly IDynamicRepository repository;
    private readonly Fanex.Logging.ILogger logger;
    private readonly IConfiguration configuration;
    private readonly ITransactionLogService transactionLogService;

    public AccumulatedCreditService(
        IDynamicRepository repository,
        Fanex.Logging.ILogger logger,
        IConfiguration configuration,
        ITransactionLogService transactionLogService)
    {
        this.repository = repository;
        this.logger = logger;
        this.configuration = configuration;
        this.transactionLogService = transactionLogService;
    }

    public async Task AccumulateCreditToAgent(bool isTest, int size)
    {
        var category = configuration.GetValue<string>("JobsSettings:NotifyCategory");
        await logger.ErrorAsync($"Start {JobName}.", category);

        var winlossCount = 0;
        var winlossJson = await GetMemberYesterdayWinLoss(isTest);

        if (!string.IsNullOrWhiteSpace(winlossJson))
        {
            var winloss = JsonSerializer.Deserialize<MemberYesterdayWL[]>(winlossJson);
            winlossCount = winloss?.Length ?? 0;

            if (winlossCount > 0)
            {
                var winlossBatches = winloss.Chunk(size);
                await Task.WhenAll(winlossBatches.Select(ProcessMemberYesterdayWinLoss));
            }
        }

        await logger.ErrorAsync($"{JobName} Cash Member Yesterday WinLoss Count: {winlossCount}.", category);
        await logger.ErrorAsync($"{JobName} completed.", category);
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
            await logger.ErrorAsync($"{criteria.GetSettingKey()} failed. CashAccumCreditTbl: {cashAccumCreditTbl}, ErrCode: {criteria.ErrCode}, ErrMsg: {criteria.ErrMsg}");
        }

        return result;
    }
}

