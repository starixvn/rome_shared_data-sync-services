using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoTransfer.Criteria.Notifications;

public class MarkNotifiedCriteria : CriteriaBase
{
    public MarkNotifiedCriteria(string siteDB, int custId)
    {
        SiteDB = siteDB;
        CustId = custId.ToString();
    }

    [SpParam(Ignored = true)]
    public string SiteDB { get; }

    [SpParam(SqlParamName = "TargetIds")]
    public string CustId { get; }

    public int ErrCode { get; }

    public string ErrMsg { get; }

    public override string GetSettingKey() => $"Age_ClearBalanceCust_Job_SetNotify_{SiteDB}";

    public override bool IsValid() => true;
}
