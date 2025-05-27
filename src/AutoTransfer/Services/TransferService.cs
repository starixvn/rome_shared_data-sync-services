using AutoTransfer.Criteria;
using AutoTransfer.Models;
using Fanex.Data.Repository;

namespace AutoTransfer.Services;

public interface ITransferService
{
    Task<ResultModel> ResetPotentialWinLimit(string custIds);

    Task<AutoTransferStartModel> Start(int pairOfBatch, string runningMode, string env);
    Task<IEnumerable<BatchModel>> SelectBatches(int numberOfThreads, string runningMode, string env);
    Task<ResultModel> ProcessBatch(BatchModel batch, string env);
    Task<RetryBatchesModel> SelectRetryBatches(int numberOfThreads, string env);
    Task<ResultModel> ProcessRetryBatch(BatchModel batch, string env);
    Task<ResultModel> ImportLogs(string env);
    Task<JobConfigModel> GetJobConfigModel(string env);
    Task<NotifyStatusModel?> GetNotifyStatusModel();
    Task<ResultModel> UpdateNotifyStatus();
}

public sealed class TransferService : ITransferService
{
    private readonly IDynamicRepository dynamicRepository;

    public TransferService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<ResultModel> ResetPotentialWinLimit(string custIds)
    {
        var criteria = new SelectMemberWinLimitCriteria(custIds);
        var ids = await dynamicRepository.FetchAsync<int>(criteria);

        if (ids.Any())
        {
            var resetCriteria = new ResetMemberWinLimitCriteria(ids);
            await dynamicRepository.ExecuteAsync(resetCriteria);

            return new ResultModel
            {
                ErrCode = resetCriteria.ErrCode,
                ErrMsg = resetCriteria.ErrMsg
            };
        }

        return new ResultModel
        {
            ErrCode = 0,
            ErrMsg = "No records found"
        };
    }


    public async Task<AutoTransferStartModel> Start(int pairOfBatch, string runningMode, string env)
    {
        var criteria = new AutoTransferStartCriteria(pairOfBatch, runningMode, env);

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

    public async Task<IEnumerable<BatchModel>> SelectBatches(int numberOfThreads, string runningMode, string env)
    {
        var criteria = new SelectBatchesCriteria(numberOfThreads, runningMode, env);

        return await dynamicRepository.FetchAsync<BatchModel>(criteria);
    }

    public async Task<ResultModel> ProcessBatch(BatchModel batch, string env)
    {
        var result = await ResetPotentialWinLimit(batch.ToCustIds);

        if (result.ErrCode == 0)
        {
            var criteria = new ProcessBatchCriteria(batch, env);
            await dynamicRepository.ExecuteAsync(criteria);

            result = new ResultModel
            {
                ErrCode = criteria.ErrCode,
                ErrMsg = criteria.ErrMsg,
            };
            return result;
        }

        return result;
    }

    public async Task<RetryBatchesModel> SelectRetryBatches(int numberOfThreads, string env)
    {
        var criteria = new SelectRetryBatchesCriteria(numberOfThreads, env);

        var batches = await dynamicRepository.FetchAsync<BatchModel>(criteria);

        return new RetryBatchesModel
        {
            Batches = batches,
            IsCompleted = criteria.IsCompleted
        };
    }

    public async Task<ResultModel> ProcessRetryBatch(BatchModel batch, string env)
    {
        var criteria = new ProcessBatchCriteria(batch, env);

        await dynamicRepository.ExecuteAsync(criteria);

        return new ResultModel
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg,
        };
    }

    public async Task<ResultModel> ImportLogs(string env)
    {

        var messages = new List<string>();

        try
        {
            var monitorLogsCriteria = new ImportMonitorLogCriteria(env);
            var logs = await dynamicRepository.FetchAsync<AutoTransferLogModel>(monitorLogsCriteria);

            foreach (var log in logs)
            {
                var importHistoryCriteria = new ImportHistoryLogCriteria(log.RowData);
                await dynamicRepository.ExecuteAsync(importHistoryCriteria);

                if (importHistoryCriteria.ErrCode != 0)
                {
                    messages.Add(importHistoryCriteria.ErrMsg);
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

    public async Task<JobConfigModel> GetJobConfigModel(string env)
    {
        var criteria = new GetJobConfigCriteria(env);
        var data = await dynamicRepository.FetchAsync<JobConfigModel>(criteria);

        return data?.FirstOrDefault() ?? JobConfigModel.CreateDefaultConfig();
    }

    public async Task<NotifyStatusModel?> GetNotifyStatusModel()
    {
        var criteria = new GetNotifyStatusCriteria();
        var data = await dynamicRepository.FetchAsync<NotifyStatusModel>(criteria);

        return data?.FirstOrDefault();
    }

    public async Task<ResultModel> UpdateNotifyStatus()
    {
        var criteria = new UpdateNotifyStatusCriteria();
        await dynamicRepository.ExecuteAsync(criteria);

        return new ResultModel
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg
        };
    }
}
