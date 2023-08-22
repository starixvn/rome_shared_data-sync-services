using AutoTransfer.Models;
using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class ProcessBatchCriteria : NonQueryCommand
{
    public ProcessBatchCriteria(BatchModel batch)
    {
        BatchNo = batch.BatchNo;
        FromCustId = batch.FromCustId;
        ToCustIds = batch.ToCustIds;
        RunningMode = batch.RunningMode;
    }

    public int BatchNo { get; set; }
    public int FromCustId { get; set; }
    public string ToCustIds { get; set; }
    public string RunningMode { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_BatchProcess";

    public override bool IsValid() => BatchNo > 0 && FromCustId > 0;
}
