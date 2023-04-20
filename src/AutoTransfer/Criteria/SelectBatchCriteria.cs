using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class SelectBatchCriteria : CriteriaBase
{
    public SelectBatchCriteria(string runningMode)
    {
        RunningMode = runningMode;
    }

    [SpParam(Ignored = true)]
    public string RunningMode { get; set; }

    public bool IsFinished { get; set; }

    [SpParam(Ignored = true)]
    public int ErrCode { get; set; }

    [SpParam(Ignored = true)]
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_BatchSel_PerfTest";

    public override bool IsValid() => true;
}
