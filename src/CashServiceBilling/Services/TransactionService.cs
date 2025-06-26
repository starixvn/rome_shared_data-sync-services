using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Fanex.Data.Repository;

namespace CashServiceBilling.Services;

public interface ITransactionService
{
    Task UpdateExpiredStatus(bool isTest);
}

public class TransactionService : ITransactionService
{
    private readonly IDynamicRepository repository;
    private readonly ITransactionLogService transactionLogService;

    public TransactionService(IDynamicRepository repository, ITransactionLogService transactionLogService)
    {
        this.repository = repository;
        this.transactionLogService = transactionLogService;
    }

    public async Task UpdateExpiredStatus(bool isTest)
    {
        var logs = await repository.FetchAsync<LogEntry>(new UpdateTransExpiredStatusCriteria(isTest));

        if (logs.Any())
        {
            await transactionLogService.WriteLogs(logs);
        }
    }
}