using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class AddManualDepositTransCriteria : CriteriaBase
{
    public AddManualDepositTransCriteria(string custIdList)
    {
        CustIdList = custIdList;
    }

    public string CustIdList { get; }

    public string CashAccumCreditTbl { get; set; }

    public override string GetSettingKey()
        => "Task_CashService_AccumCreditReturn_YesterdayLoss_Adjust";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(CustIdList);
}