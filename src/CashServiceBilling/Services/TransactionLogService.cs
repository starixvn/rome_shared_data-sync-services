using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;

namespace CashServiceBilling.Services;

public interface ITransactionLogService
{
    Task WriteLogs(IEnumerable<LogEntry> logs);

    Task WriteLogs(IEnumerable<LogEntry> logs, short custDBGroupId);

    Task<bool> WriteLog(INonQueryCommand criteria);
}

public class TransactionLogService : ITransactionLogService
{
    private readonly IDynamicRepository repository;
    private readonly Fanex.Logging.ILogger logger;

    public TransactionLogService(IDynamicRepository repository, Fanex.Logging.ILogger logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task WriteLogs(IEnumerable<LogEntry> logs)
    {
        var logTasks = logs.GroupBy(l => l.CustDBGroupId, (key, group) => WriteLogs(group, key));
        await Task.WhenAll(logTasks);
    }

    public async Task WriteLogs(IEnumerable<LogEntry> logs, short custDBGroupId)
    {
        foreach (var item in logs)
        {
            var criteria = new WriteLogRunSPCriteria(
                item.UserId,
                item.CustId,
                item.ActionId,
                item.TrackingValueMask,
                item.TrackingValueMask2,
                item.TrackingValue,
                item.TrackingValue2);
            var isSuccess = await WriteLog(criteria);

            if (!isSuccess)
            {
                criteria.CustDBGroupId = custDBGroupId;
                await WriteLog(criteria);
            }
        }
    }

    public async Task<bool> WriteLog(INonQueryCommand criteria)
    {
        try
        {
            await repository.ExecuteAsync(criteria);
            return true;
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync($"{criteria.GetSettingKey()} failed.", ex);
            return false;
        }
    }
}