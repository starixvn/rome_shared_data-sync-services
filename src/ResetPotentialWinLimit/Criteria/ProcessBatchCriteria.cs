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
    public string CustList { get; }

    public override string GetSettingKey() => "Acc_CustWinLimitBet_Reset";

    public override bool IsValid() => !string.IsNullOrEmpty(CustList);
}
