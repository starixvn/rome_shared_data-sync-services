using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoTransfer.Criteria.Notifications;

public class AssignPersonalMessageCriteria : CriteriaBase
{
    public AssignPersonalMessageCriteria(long messageId, int custId, string custName)
    {
        MessageId = messageId;
        CustId = custId;
        CustName = custName;
        BetTransId = "0";
    }

    public long MessageId { get; }

    [SpParam(SqlParamName = "TargetId")]
    public int CustId { get; }

    [SpParam(SqlParamName = "TargetName")]
    public string CustName { get; }

    public string BetTransId { get; }

    public override string GetSettingKey() => "Age_Msg_SetUsrMessage";

    public override bool IsValid() => MessageId > 0;
}
