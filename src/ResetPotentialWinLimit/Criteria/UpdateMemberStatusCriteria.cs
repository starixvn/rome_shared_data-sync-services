using Fanex.Data;
using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class UpdateMemberStatusCriteria : NonQueryCommand
{
    public UpdateMemberStatusCriteria(string custList, bool isCompleted)
    {
        CustList = custList;
        IsCompleted = isCompleted;
    }

    public string CustList { get; set; }

    [SpParam(Ignored = true)]
    public bool IsCompleted { get; set; }

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "TaskDailyResetAccumWinLoss_MemberStatusUpd";

    public override bool IsValid() => !string.IsNullOrEmpty(CustList);
}
