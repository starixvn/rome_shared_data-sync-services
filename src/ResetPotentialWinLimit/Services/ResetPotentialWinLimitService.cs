using Fanex.Data.Repository;
using Newtonsoft.Json;
using ResetPotentialWinLimit.Criteria;
using ResetPotentialWinLimit.Models;

namespace ResetPotentialWinLimit.Services;

public interface IResetPotentialWinLimitService
{
    Task<JobConfigModel> GetJobConfig();
    Task<JobStatusModel> GetJobStatus();
    Task UpdateJobStatus(JobStatusModel jobStatus);
    Task<MemberBatchModel> SelectMemberList(int batchSize, int lastMaxCustID);
    Task<ResultModel> ProcessBatch(string custList);
}

public sealed class ResetPotentialWinLimitService : IResetPotentialWinLimitService
{
    private readonly IDynamicRepository dynamicRepository;

    public ResetPotentialWinLimitService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<JobConfigModel> GetJobConfig()
    {
        var criteria = new GetJobStatusCommand("ResetPotentialWinLimitJob_Config");
        await dynamicRepository.FetchAsync<JobConfigModel>(criteria);
        var jobConfig =
            string.IsNullOrWhiteSpace(criteria.VValue) ?
            JobConfigModel.CreateDefault() :
            JsonConvert.DeserializeObject<JobConfigModel>(criteria.VValue);

        return jobConfig;
    }

    public async Task<JobStatusModel> GetJobStatus()
    {
        var criteria = new GetJobStatusCommand("ResetPotentialWinLimitJob_Status");
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
        var criteria = new UpdateJobStatusCommand("ResetPotentialWinLimitJob_Status", json);
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
        var criteria = new ProcessBatchCriteria(custList);
        await dynamicRepository.ExecuteAsync(criteria);

        return new ResultModel();
    }
}
