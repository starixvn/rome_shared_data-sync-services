using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;

namespace CashServiceBilling.Services;

public interface ITransactionLogService
{
    Task WriteLogs(IEnumerable<LogEntry> logs, int batch = 500);

    Task WriteLogs(IEnumerable<LogEntry> logs, short custDBGroupId, int batch);

    Task WriteLog(string logData, short custDBGroupId);

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

    public async Task WriteLogs(IEnumerable<LogEntry> logs, int batch = 500)
    {
        var logTasks = logs.GroupBy(l => l.CustDBGroupId);

        foreach (var group in logTasks)
        {
            await WriteLogs(group, group.Key, batch);
        }
    }

    public async Task WriteLogs(IEnumerable<LogEntry> logs, short custDBGroupId, int batch)
    {
        var logsBatch = logs.Chunk(batch);

        foreach (var item in logsBatch)
        {
            var criteria = new WriteLogRunSPCriteria(JsonSerializer.Serialize(item));
            var isSuccess = await WriteLog(criteria);

            if (!isSuccess)
            {
                criteria.CustDBGroupId = custDBGroupId;
                await WriteLog(criteria);
            }
        }
    }

    public async Task WriteLog(string logData, short custDBGroupId)
    {
        var criteria = new WriteLogRunSPCriteria(logData);
        var isSuccess = await WriteLog(criteria);

        if (!isSuccess)
        {
            criteria.CustDBGroupId = custDBGroupId;
            await WriteLog(criteria);
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
