using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class GetAgentSettingBonusCriteria : NonQueryCommand
{
    public GetAgentSettingBonusCriteria(bool isTest)
    {
        IsTest = isTest;
    }

    public bool IsTest { get; }

    public string AgentPromotion { get; set; }

    public override string GetSettingKey() => "Task_CashService_AgentSetting_Sel";

    public override bool IsValid() => true;
}