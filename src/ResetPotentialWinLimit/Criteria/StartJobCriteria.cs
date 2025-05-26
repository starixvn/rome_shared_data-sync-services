using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class StartJobCriteria : NonQueryCommand
{
    public StartJobCriteria() { }

    public override string GetSettingKey() => "TaskDailyResetAccumWinLoss_JobStart";

    public override bool IsValid() => true;
}
