using Fanex.Data;
using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class WriteLogRunSPCriteria : NonQueryCommand
{
    public WriteLogRunSPCriteria(
        int userId,
        int custId,
        int actionId,
        string trackingValueMask,
        string trackingValueMask2,
        string trackingValue,
        string trackingValue2,
        short? custDBGroupId = null)
    {
        UserId = userId;
        CustId = custId;
        ActionId = actionId;
        TrackingValueMask = trackingValueMask;
        TrackingValueMask2 = trackingValueMask2;
        TrackingValue = trackingValue;
        TrackingValue2 = trackingValue2;
        CustDBGroupId = custDBGroupId;
    }

    public int UserId { get; }

    public int CustId { get; }

    public int ActionId { get; }

    public string TrackingValueMask { get; }

    public string TrackingValueMask2 { get; }

    public string TrackingValue { get; }

    public string TrackingValue2 { get; }

    [SpParam(Ignored = true)]
    public short? CustDBGroupId { get; set; }

    public override string GetSettingKey()
        => CustDBGroupId.HasValue
        ? $"CustDB_{CustDBGroupId}_WriteLog_CashService_UserLog_RunSP"
        : "WriteLog_CashService_UserLog_RunSP";
    public override bool IsValid() => CustId > 0;
}
