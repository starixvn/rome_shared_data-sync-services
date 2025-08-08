using Fanex.Data;
using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class WriteLogRunSPCriteria : NonQueryCommand
{
    public WriteLogRunSPCriteria(string logDataMultiple)
    {
        LogData_Multiple = logDataMultiple;
    }

    public string LogData_Multiple { get; }

    [SpParam(Ignored = true)]
    public short? CustDBGroupId { get; set; }

    public override string GetSettingKey()
        => CustDBGroupId.HasValue
        ? $"CustDB_{CustDBGroupId}_WriteLog_CashService_UserLog_RunSP"
        : "WriteLog_CashService_UserLog_RunSP";
    public override bool IsValid() => !string.IsNullOrWhiteSpace(LogData_Multiple);
}
