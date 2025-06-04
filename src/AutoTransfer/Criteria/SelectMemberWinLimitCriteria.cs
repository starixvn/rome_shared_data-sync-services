using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class SelectMemberWinLimitCriteria : CriteriaBase
{
    public SelectMemberWinLimitCriteria(string custIds)
    {
        ListCustId = custIds;
    }

    public string ListCustId { get; set; }

    public override string GetSettingKey() => "Acc_CustPotentialWinLimit_SelAutoResetMembers";

    public override bool IsValid() => !string.IsNullOrEmpty(ListCustId);
}
