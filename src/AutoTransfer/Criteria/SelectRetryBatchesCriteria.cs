using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class SelectRetryBatchesCriteria : CriteriaBase
{
    public SelectRetryBatchesCriteria(int numberOfThreads)
    {
        NumOfThread = numberOfThreads;
    }

    public int NumOfThread { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
    public bool IsCompleted { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_ReTrySel";

    public override bool IsValid() => NumOfThread > 0;
}
