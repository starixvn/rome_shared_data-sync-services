namespace AutoTransfer.Models;

public class JobsSettingsModel
{
    public int PairOfBatch { get; set; }
    public int NumberOfThreads { get; set; }
    public int DelayExecuteBatchesTime { get; set; }
    public int DelayRetryJobTimeInMilliSeconds { get; set; }
    public bool IgnoreSystemUM { get; set; }
}
