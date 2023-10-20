using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class ImportMonitorLogCriteria : CriteriaBase
{
    public ImportMonitorLogCriteria(string env)
    {
        Environment = env;
    }

    public string Environment { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_ProcessExport";

    public override bool IsValid() => true;
}
