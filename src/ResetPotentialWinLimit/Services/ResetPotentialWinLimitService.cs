using Fanex.Data.Repository;
using Newtonsoft.Json;
using ResetPotentialWinLimit.Criteria;
using ResetPotentialWinLimit.Models;

namespace ResetPotentialWinLimit.Services;

public interface IResetPotentialWinLimitService
{
    Task<JobStatusModel> GetJobStatus();
    Task UpdateJobStatus(JobStatusModel jobStatus);
    Task<MemberBatchModel> SelectMemberList(int batchSize, int lastMaxCustID);
    Task<ResultModel> ProcessBatch(string custList);
}

public sealed class ResetPotentialWinLimitService : IResetPotentialWinLimitService
{
    private const string VName = "ResetPotentialWinLimitJob";
    private readonly IDynamicRepository dynamicRepository;

    public ResetPotentialWinLimitService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<JobStatusModel> GetJobStatus()
    {
        var criteria = new GetJobStatusCommand(VName);
        await dynamicRepository.FetchAsync<JobStatusModel>(criteria);
        var jobStatus =
            string.IsNullOrWhiteSpace(criteria.VValue) ?
            JobStatusModel.CreateDefault() :
            JsonConvert.DeserializeObject<JobStatusModel>(criteria.VValue);

        return jobStatus;
    }

    public async Task UpdateJobStatus(JobStatusModel jobStatus)
    {
        var json = JsonConvert.SerializeObject(jobStatus);
        var criteria = new UpdateJobStatusCommand(VName, json);
        await dynamicRepository.ExecuteAsync(criteria);
    }

    public async Task<MemberBatchModel> SelectMemberList(int batchSize, int lastMaxCustID)
    {
        var criteria = new SelectMemberListCriteria(batchSize, lastMaxCustID);

        var memberIds = await dynamicRepository.FetchAsync<int>(criteria);

        return new MemberBatchModel
        {
            MemberIds = memberIds,
            LastMaxCustID = criteria.LastMaxCustID,
        };
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
}
