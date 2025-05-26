using Fanex.Data;
using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class ProcessBatchCriteria : NonQueryCommand
{
    public ProcessBatchCriteria(string custList)
    {
        CustList = custList;
    }

    [SpParam(SqlParamName = "ListCustId")]
    public string CustList { get; set; }

    public override string GetSettingKey() => "ResetPotentialWinLimit_Job_Process";

    public override bool IsValid() => !string.IsNullOrEmpty(CustList);
}
