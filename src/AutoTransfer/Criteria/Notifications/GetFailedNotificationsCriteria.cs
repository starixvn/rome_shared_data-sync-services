using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoTransfer.Criteria.Notifications;

public class GetFailedNotificationsCriteria : CriteriaBase
{
    public GetFailedNotificationsCriteria(string siteDB)
    {
        SiteDB = siteDB;
    }

    [SpParam(Ignored = true)]
    public string SiteDB { get; set; }

    public override string GetSettingKey() => $"Age_ClearBalanceCust_Job_GetNotify_{SiteDB}";

    public override bool IsValid() => true;
}
