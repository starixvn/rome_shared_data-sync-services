namespace AutoTransfer.Models;

public class RetryBatchesModel
{
    public bool IsCompleted { get; set; }
    public IEnumerable<BatchModel> Batches { get; set; }
}
