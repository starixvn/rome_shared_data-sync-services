namespace ResetPotentialWinLimit.Models;

public class JobsSettingsModel
{
    public int BatchSize { get; set; }
    public int NumberOfThreads { get; set; }
    public int DelayExecuteBatchesTimeInMilliSeconds { get; set; }
    public int StartJobCheckingIntervalInMilliSeconds { get; set; }
    public int DelayRetryInMilliSeconds { get; set; }
    public string NotifyCategory { get; set; }
    public string SiteDBName { get; set; }
    public string[] SiteDBs { get; set; }
    public string Environment { get; set; }
}
