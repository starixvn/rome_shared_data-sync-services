using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class AddBonusTransactionCriteria : NonQueryCommand
{
    public AddBonusTransactionCriteria(string memberBonus, int siteDBGroupId)
    {
        MemberBonus = memberBonus;
        SiteDBGroupId = siteDBGroupId;
    }

    public string MemberBonus { get; }

    public int SiteDBGroupId { get; }

    public override string GetSettingKey() => $"SiteDB_{SiteDBGroupId}_Task_CashService_BonusTransaction_Sel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(MemberBonus);
}
