using Fanex.Data.Repository;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;

namespace CashServiceBilling.Services;

public interface ICashServiceBillingService
{
    JobConfigModel GetJobConfig();
    Task<IEnumerable<BatchModel>> GetAgentList(IEnumerable<int> siteDBGroupIds);
    Task<ResultModel> StartJob(BatchModel batchModel);
}

public sealed class CashServiceBillingService : ICashServiceBillingService
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly Fanex.Logging.ILogger logger;

    public CashServiceBillingService(IDynamicRepository dynamicRepository, Fanex.Logging.ILogger logger)
    {
        this.dynamicRepository = dynamicRepository;
        this.logger = logger;
    }

    public JobConfigModel GetJobConfig()
    {
        return new JobConfigModel();
    }

    public async Task<IEnumerable<BatchModel>> GetAgentList(IEnumerable<int> siteDBGroupIds)
    {
        var criteria = new GetAgentListCriteria(siteDBGroupIds);
        var agentList = await dynamicRepository.FetchAsync<BatchModel>(criteria);
        return agentList;
    }

    public async Task<ResultModel> StartJob(BatchModel batchModel)
    {
        var criteria = new StartJobCriteria(batchModel);

        var logs = await dynamicRepository.FetchAsync<LogModel>(criteria);

        foreach (var log in logs)
        {
            await WriteLog(batchModel.SiteDBGroupId, log);
        }

        return new ResultModel
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg,
        };
    }

    public async Task WriteLog(int siteDBGroupId, LogModel logModel)
    {
        var isSuccess = false;

        try
        {
            var criteria = new WriteLogCriteria(siteDBGroupId: -1, logModel);
            await dynamicRepository.ExecuteAsync(criteria);
            isSuccess = true;
        }
        catch (Exception e)
        {
            await logger.ErrorAsync(e.Message, e);
        }

        if (isSuccess)
        {
            return;
        }

        try
        {
            var criteria = new WriteLogCriteria(siteDBGroupId, logModel);
            await dynamicRepository.ExecuteAsync(criteria);
        }
        catch (Exception e)
        {
            await logger.ErrorAsync(e.Message, e);
        }
    }
}
