using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class SelectSuperListCriteria : CriteriaBase
{
    public SelectSuperListCriteria(int batchSize)
    {
        BatchSize = batchSize;
    }

    public int BatchSize { get; set; }

    public override string GetSettingKey() => "TaskDailyResetAccumWinLoss_SuperListSel";

    public override bool IsValid() => BatchSize > 0;
}
