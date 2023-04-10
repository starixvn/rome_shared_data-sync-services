namespace AutoTransfer.Models;

public class AutoTransferStartModel
{
    public int TotalPair { get; set; }
    public int TotalBatch { get; set; }
    public int TotalSuper { get; set; }
    public string RunningMode { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
}
