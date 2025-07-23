using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class GetBonusTransactionCriteria : NonQueryCommand
{
    public GetBonusTransactionCriteria(int siteDBGroupId, int maxCustId)
    {
        SiteDBGroupId = siteDBGroupId;
        MaxCustId = maxCustId;
    }

    public DateTime TransDate { get; } = DateTime.Today;
    public int SiteDBGroupId { get; }
    public string MemberBonus { get; set; }
    public int MaxCustId { get; set; }

    public override string GetSettingKey() => "Task_CashService_BonusTransaction_Sel";

    public override bool IsValid() => true;
}