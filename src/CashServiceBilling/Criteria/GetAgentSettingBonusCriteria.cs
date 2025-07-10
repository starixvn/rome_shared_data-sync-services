using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class GetAgentSettingBonusCriteria : NonQueryCommand
{
    public GetAgentSettingBonusCriteria(bool isTest, DateTime mondayLastWeek, DateTime sundayLastWeek)
    {
        IsTest = isTest;
        MondayLastWeek = mondayLastWeek;
        SundayLastWeek = sundayLastWeek;
    }

    public bool IsTest { get; }

    public DateTime MondayLastWeek { get; }

    public DateTime SundayLastWeek { get; }

    public string AgentPromotion { get; set; }

    public override string GetSettingKey() => "Task_CashService_AgentSetting_Sel";

    public override bool IsValid() => true;
}