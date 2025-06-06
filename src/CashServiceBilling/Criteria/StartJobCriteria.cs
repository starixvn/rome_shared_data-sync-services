using CashServiceBilling.Models;
using Fanex.Data;
using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class StartJobCriteria : CriteriaBase
{
    public StartJobCriteria(BatchModel batchModel)
    {
        SiteDBGroupId = batchModel.SiteDBGroupId;
        AgentList = batchModel.AgentList;
    }

    [SpParam(Ignored = true)]
    public int SiteDBGroupId { get; set; }
    public string AgentList { get; }

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => $"SiteDB_{SiteDBGroupId}_Age_CashServiceBilling_Job_Start";

    public override bool IsValid() => true;
}
