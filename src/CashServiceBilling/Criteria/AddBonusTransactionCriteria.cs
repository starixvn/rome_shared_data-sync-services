using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class AddBonusTransactionCriteria : NonQueryCommand
{
    public AddBonusTransactionCriteria(string memberBonus, int siteDBGroupId, DateTime transDate)
    {
        MemberBonus = memberBonus;
        SiteDBGroupId = siteDBGroupId;
        TransDate = transDate;
    }

    public string MemberBonus { get; }

    public int SiteDBGroupId { get; }

    public DateTime TransDate { get; }

    public bool IsBonusChanged { get; set; }

    public override string GetSettingKey() => $"SiteDB_{SiteDBGroupId}_Task_CashService_BonusTransaction_Ins";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(MemberBonus);
}
