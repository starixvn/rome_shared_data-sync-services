using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class GetCashMemberListCriteria : NonQueryCommand
{
    public GetCashMemberListCriteria(bool isTest)
    {
        IsTest = isTest;
    }

    public bool IsTest { get; }

    public string CustIdList { get; set; }

    public override string GetSettingKey()
        => "Task_CashService_AccumCreditReturn_YesterdayLoss_Sel";

    public override bool IsValid() => true;
}