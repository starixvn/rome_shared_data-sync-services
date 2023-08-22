using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class ImportHistoryLogCriteria : NonQueryCommand
{
    public ImportHistoryLogCriteria(string rowData, int replicateNo)
    {
        RowData = rowData;
        ReplicateNo = replicateNo;
    }

    public string RowData { get; }

    [SpParam(Ignored = true)]
    public int ReplicateNo { get; }

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() =>
        ReplicateNo == 0
        ? "Age_ClearBalanceCust_Job_ProcessHistoryImport"
        : "Age_ClearBalanceCust_Job_ProcessHistoryImport_HA" + ReplicateNo;

    public override bool IsValid() => !string.IsNullOrWhiteSpace(RowData);
}
