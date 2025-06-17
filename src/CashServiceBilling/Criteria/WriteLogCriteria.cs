using CashServiceBilling.Models;
using Fanex.Data;
using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class WriteLogCriteria : NonQueryCommand
{
    public WriteLogCriteria(int siteDBGroupId, LogModel logModel)
    {
        SiteDBGroupId = siteDBGroupId;
        LogData = logModel.LogData;
    }

    [SpParam(Ignored = true)]
    public int SiteDBGroupId { get; set; }

    public string LogData { get; set; }

    public override string GetSettingKey() => $"SiteDB_{SiteDBGroupId}_WriteLog_CashService_UserLog_Transfer";

    public override bool IsValid() => true;
}
