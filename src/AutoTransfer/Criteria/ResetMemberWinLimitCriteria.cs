using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class ResetMemberWinLimitCriteria : NonQueryCommand
{
    public ResetMemberWinLimitCriteria(IEnumerable<int> custIds)
    {
        ListCustId = custIds != null ? string.Join(",", custIds) : string.Empty;
    }

    public string ListCustId { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "CustWinLimitBet_Reset";

    public override bool IsValid() => !string.IsNullOrEmpty(ListCustId);
}
