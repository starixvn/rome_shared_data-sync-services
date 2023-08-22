using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class GetJobConfigCriteria : CriteriaBase
{
    public GetJobConfigCriteria() { }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_ConfigSel";

    public override bool IsValid() => true;
}
