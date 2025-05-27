using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class ResetMemberWinLimitCriteria : NonQueryCommand
{
    public ResetMemberWinLimitCriteria(IEnumerable<int> custIds)
    {
        CustIds = string.Join(",", custIds);
    }

    public string CustIds { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "CustWinLimitBet_Reset";

    public override bool IsValid() => !string.IsNullOrEmpty(CustIds);
}
