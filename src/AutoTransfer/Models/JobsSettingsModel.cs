namespace AutoTransfer.Models;

public class JobsSettingsModel
{
    public int PairOfBatch { get; set; }
    public int NumberOfThreads { get; set; }
    public int DelayExecuteBatchesTimeInMilliSeconds { get; set; }
    public int DelayRetryJobTimeInMilliSeconds { get; set; }
    public int HistoryReplicateNumber { get; set; }

    public int StartJobCheckingIntervalInMilliSeconds { get; set; }
}
