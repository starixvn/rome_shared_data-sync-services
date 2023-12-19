using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class GetNotifyStatusCriteria : CriteriaBase
{
    public GetNotifyStatusCriteria() { }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_NotifySel";

    public override bool IsValid() => true;
}
