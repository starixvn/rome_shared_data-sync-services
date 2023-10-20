using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class AutoTransferStartCriteria : NonQueryCommand
{
    public AutoTransferStartCriteria(int pairOfBatch, string runningMode, string env)
    {
        PairOfBatch = pairOfBatch;
        RunningMode = runningMode;
        Environment = env;
    }

    public int PairOfBatch { get; set; }
    public int TotalPair { get; set; }
    public int TotalBatch { get; set; }
    public int TotalSuper { get; set; }
    public string RunningMode { get; set; }
    public string Environment { get; set; }

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_Start";

    public override bool IsValid() => PairOfBatch > 0;
}
