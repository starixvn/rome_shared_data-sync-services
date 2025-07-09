using Fanex.Data;
using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class AddBonusTransactionCriteria : NonQueryCommand
{
    public AddBonusTransactionCriteria(string agentPromotion, short siteDBGroupId)
    {
        AgentPromotion = agentPromotion;
        SiteDBGroupId = siteDBGroupId;
    }

    public string AgentPromotion { get; }

    [SpParam(Ignored = true)]
    public short SiteDBGroupId { get; set; }

    public override string GetSettingKey() => $"SiteDB_{SiteDBGroupId}_Task_CashService_BonusTransaction_Ins";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(AgentPromotion);
}