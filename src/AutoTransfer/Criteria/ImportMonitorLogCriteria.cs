using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class ImportMonitorLogCriteria : CriteriaBase
{
    public ImportMonitorLogCriteria()
    {
    }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_ProcessExport";

    public override bool IsValid() => true;
}
