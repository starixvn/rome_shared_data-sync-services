using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class SelectMemberListCriteria : CriteriaBase
{
    public SelectMemberListCriteria(int superId)
    {
        SuperId = superId;
    }

    public int SuperId { get; set; }

    public override string GetSettingKey() => "TaskDailyResetAccumWinLoss_MemberListSel";

    public override bool IsValid() => SuperId > 0;
}
