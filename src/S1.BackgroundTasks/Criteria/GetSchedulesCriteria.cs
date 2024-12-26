using Fanex.Data.Repository;

namespace S1.BackgroundTasks.Criteria
{
    public class GetSchedulesCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "Task_WhiteLabelWinloss_GetSchedule";

        public override bool IsValid() => true;
    }
}