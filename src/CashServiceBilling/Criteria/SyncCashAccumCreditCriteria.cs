using Fanex.Data;
using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class SyncCashAccumCreditCriteria : NonQueryCommand
{
    public SyncCashAccumCreditCriteria(string cashAccumCreditTbl, short custDBGroupId)
    {
        CashAccumCreditTbl = cashAccumCreditTbl;
        CustDBGroupId = custDBGroupId;
    }

    public string CashAccumCreditTbl { get; }

    [SpParam(Ignored = true)]
    public short CustDBGroupId { get; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => $"CustDB_{CustDBGroupId}_Age_CashAccumCredit_SyncData";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(CashAccumCreditTbl);
}