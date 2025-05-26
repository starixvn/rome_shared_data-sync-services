using Fanex.Data.Repository;
using ResetPotentialWinLimit.Criteria;
using ResetPotentialWinLimit.Models;

namespace ResetPotentialWinLimit.Services;

public interface IResetPotentialWinLimitService
{
    Task Start();
    JobConfigModel GetJobConfigModel(string env);
    Task<IEnumerable<int>> SelectSuperList(int batchSize);
    Task<IEnumerable<int>> SelectMemberList(int superId);
    Task<ResultModel> ProcessBatch(string custList);
    Task<ResultModel> UpdateSuperStatus(int custId, bool isCompleted);
    Task<ResultModel> UpdateMemberStatus(string custList, bool isCompleted);
}

public sealed class ResetPotentialWinLimitService : IResetPotentialWinLimitService
{
    private readonly IDynamicRepository dynamicRepository;

    public ResetPotentialWinLimitService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task Start()
    {
        var criteria = new StartJobCriteria();

        await dynamicRepository.ExecuteAsync(criteria);
    }

    public JobConfigModel GetJobConfigModel(string env)
    {
        return JobConfigModel.CreateDefaultConfig();
    }

    public async Task<IEnumerable<int>> SelectSuperList(int batchSize)
    {
        var criteria = new SelectSuperListCriteria(batchSize);

        return await dynamicRepository.FetchAsync<int>(criteria);
    }

    public async Task<IEnumerable<int>> SelectMemberList(int superId)
    {
        var criteria = new SelectMemberListCriteria(superId);

        return await dynamicRepository.FetchAsync<int>(criteria);
    }


    public async Task<ResultModel> ProcessBatch(string custList)
    {
        try
        {
            var criteria = new ProcessBatchCriteria(custList);
            await dynamicRepository.ExecuteAsync(criteria);

            return new ResultModel();
        }
        catch (Exception ex)
        {
            return new ResultModel
            {
                ErrCode = 1,
                ErrMsg = ex.Message,
            };
        }
    }

    public async Task<ResultModel> UpdateSuperStatus(int custId, bool isCompleted)
    {
        var criteria = new UpdateSuperStatusCriteria(custId, isCompleted);

        await dynamicRepository.ExecuteAsync(criteria);

        return new ResultModel
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg,
        };
    }

    public async Task<ResultModel> UpdateMemberStatus(string custList, bool isCompleted)
    {
        var criteria = new UpdateMemberStatusCriteria(custList, isCompleted);

        await dynamicRepository.ExecuteAsync(criteria);

        return new ResultModel
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg,
        };
    }
}
