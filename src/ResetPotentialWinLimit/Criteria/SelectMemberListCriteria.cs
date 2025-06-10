using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class SelectMemberListCriteria : CriteriaBase
{
    public SelectMemberListCriteria(int batchSize, int lastMaxCustID)
    {
        BatchSize = batchSize;
        LastMaxCustID = lastMaxCustID;
    }

    public int BatchSize { get; }

    public int LastMaxCustID { get; set; }

    public override string GetSettingKey() => "Task_ResetAccumWinLoss_MemberListSel";

    public override bool IsValid() => BatchSize > 0;
}
