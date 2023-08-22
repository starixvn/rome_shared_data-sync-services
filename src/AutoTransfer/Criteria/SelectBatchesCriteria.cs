using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class SelectBatchesCriteria : CriteriaBase
{
    public SelectBatchesCriteria(int numberOfThreads, string runningMode)
    {
        NumOfThread = numberOfThreads;
        RunningMode = runningMode;
    }

    public int NumOfThread { get; set; }
    public string RunningMode { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_BatchSel";

    public override bool IsValid() => NumOfThread > 0;
}
