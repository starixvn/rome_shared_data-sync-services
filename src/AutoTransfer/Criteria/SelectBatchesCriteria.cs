using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class SelectBatchesCriteria : CriteriaBase
{
    public SelectBatchesCriteria(int numberOfThreads, string runningMode, string env)
    {
        NumOfThread = numberOfThreads;
        RunningMode = runningMode;
        Environment = env;
    }

    public int NumOfThread { get; set; }
    public string RunningMode { get; set; }
    public string Environment { get; set; }

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_BatchSel";

    public override bool IsValid() => NumOfThread > 0;
}
