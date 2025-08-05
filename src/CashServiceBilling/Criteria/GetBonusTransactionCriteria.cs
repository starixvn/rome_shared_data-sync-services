using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class GetBonusTransactionCriteria : NonQueryCommand
{
    public GetBonusTransactionCriteria(int siteDBGroupId, int maxCustId, DateTime transDate)
    {
        SiteDBGroupId = siteDBGroupId;
        MaxCustId = maxCustId;
        TransDate = transDate;
    }

    public int SiteDBGroupId { get; }
    public int MaxCustId { get; set; }
    public DateTime TransDate { get; }
    public string MemberBonus { get; set; }

    public override string GetSettingKey() => "Task_CashService_BonusTransaction_Sel";

    public override bool IsValid() => true;
}