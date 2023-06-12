using AutoTransfer.Criteria;
using AutoTransfer.Models;
using Fanex.Data.Repository;

namespace AutoTransfer.Services;

public interface ITransferService
{
    Task<AutoTransferStartModel> Start(int pairOfBatch, string runningMode);
    Task<IEnumerable<BatchModel>> SelectBatches(int numberOfThreads, string runningMode);
    Task<ResultModel> ProcessBatch(BatchModel batch);
    Task<RetryBatchesModel> SelectRetryBatches(int numberOfThreads);
    Task<ResultModel> ProcessRetryBatch(BatchModel batch);
    Task<ResultModel> ImportLogs(int historyReplicateNumber);
    Task<JobConfigModel> GetJobConfigModel();
}

public sealed class TransferService : ITransferService
{
    private readonly IDynamicRepository dynamicRepository;

    public TransferService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;


    public async Task<AutoTransferStartModel> Start(int pairOfBatch, string runningMode)
    {
        var criteria = new AutoTransferStartCriteria(pairOfBatch, runningMode);

        await dynamicRepository.ExecuteAsync(criteria);

        return new AutoTransferStartModel
        {
            TotalPair = criteria.TotalPair,
            TotalBatch = criteria.TotalBatch,
            TotalSuper = criteria.TotalSuper,
            RunningMode = criteria.RunningMode,
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg
        };
    }

    public async Task<IEnumerable<BatchModel>> SelectBatches(int numberOfThreads, string runningMode)
    {
        var criteria = new SelectBatchesCriteria(numberOfThreads, runningMode);

        return await dynamicRepository.FetchAsync<BatchModel>(criteria);
    }

    public async Task<ResultModel> ProcessBatch(BatchModel batch)
    {
        var criteria = new ProcessBatchCriteria(batch);

        await dynamicRepository.ExecuteAsync(criteria);

        return new ResultModel
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg,
        };
    }

    public async Task<RetryBatchesModel> SelectRetryBatches(int numberOfThreads)
    {
        var criteria = new SelectRetryBatchesCriteria(numberOfThreads);

        var batches = await dynamicRepository.FetchAsync<BatchModel>(criteria);

        return new RetryBatchesModel
        {
            Batches = batches,
            IsCompleted = criteria.IsCompleted
        };
    }

    public async Task<ResultModel> ProcessRetryBatch(BatchModel batch)
    {
        var criteria = new ProcessBatchCriteria(batch);

        await dynamicRepository.ExecuteAsync(criteria);

        return new ResultModel
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg,
        };
    }

    public async Task<ResultModel> ImportLogs(int historyReplicateNumber)
    {

        var messages = new List<string>();

        try
        {
            var monitorLogsCriteria = new ImportMonitorLogCriteria();
            var logs = await dynamicRepository.FetchAsync<AutoTransferLogModel>(monitorLogsCriteria);

            foreach (var log in logs)
            {
                var importHistoryCriteria = new ImportHistoryLogCriteria(log.RowData, 0);
                await dynamicRepository.ExecuteAsync(importHistoryCriteria);

                if (importHistoryCriteria.ErrCode != 0)
                {
                    messages.Add(importHistoryCriteria.ErrMsg);
                }

                for (var i = 0; i < historyReplicateNumber; i++)
                {
                    var replicateCriteria = new ImportHistoryLogCriteria(log.RowData, i + 1);
                    await dynamicRepository.ExecuteAsync(replicateCriteria);

                    if (replicateCriteria.ErrCode != 0)
                    {
                        messages.Add(replicateCriteria.ErrMsg);
                    }
                }
            }
        }
        catch (Exception e)
        {
            messages.Add(e.Message);
        }

        return new ResultModel
        {
            ErrCode = messages.Any() ? 1 : 0,
            ErrMsg = string.Join("\n\r", messages)
        };
    }

    public async Task<JobConfigModel> GetJobConfigModel()
    {
        var criteria = new GetJobConfigCriteria();
        var data = await dynamicRepository.FetchAsync<JobConfigModel>(criteria);

        return data?.FirstOrDefault() ?? JobConfigModel.CreateDefaultConfig();
    }
}
