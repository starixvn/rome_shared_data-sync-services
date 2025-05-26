using Fanex.Data;
using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class UpdateSuperStatusCriteria : NonQueryCommand
{
    public UpdateSuperStatusCriteria(int custId, bool isCompleted)
    {
        CustId = custId;
        IsCompleted = isCompleted;
    }

    public int CustId { get; set; }

    [SpParam(Ignored = true)]
    public bool IsCompleted { get; set; }

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "TaskDailyResetAccumWinLoss_SuperStatusUpd";

    public override bool IsValid() => CustId > 0;
}
