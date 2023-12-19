using Fanex.Data.Repository;

namespace AutoTransfer.Criteria;

public class UpdateNotifyStatusCriteria : NonQueryCommand
{
    public UpdateNotifyStatusCriteria() { }

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Age_ClearBalanceCust_Job_NotifyUpd";

    public override bool IsValid() => true;
}
