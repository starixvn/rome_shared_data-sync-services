using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class ImportHistoryLogCriteria : NonQueryCommand
{
    public ImportHistoryLogCriteria(string rowData)
    {
        RowData = rowData;
    }

    public string RowData { get; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_ProcessHistoryImport";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(RowData);
}
