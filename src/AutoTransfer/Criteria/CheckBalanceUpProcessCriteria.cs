using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class CheckBalanceUpProcessCriteria : NonQueryCommand
{
    public CheckBalanceUpProcessCriteria()
    {
    }

    public DateTime? EndProcessingTime { get; set; }

    public override string GetSettingKey() => "Acc_CheckProcessingData";

    public override bool IsValid() => true;
}
