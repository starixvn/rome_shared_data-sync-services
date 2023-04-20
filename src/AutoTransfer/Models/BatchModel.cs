namespace AutoTransfer.Models;

public class BatchModel
{
    public int BatchNo { get; set; }
    public int FromCustId { get; set; }
    public string ToCustIds { get; set; }
    public string RunningMode { get; set; }
    public bool IsFinished { get; set; }
}
