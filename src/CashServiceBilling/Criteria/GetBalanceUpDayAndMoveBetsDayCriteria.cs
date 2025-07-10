using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class GetBalanceUpDayAndMoveBetsDayCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Age_SelBalanceUpDay";

    public override bool IsValid() => true;
}