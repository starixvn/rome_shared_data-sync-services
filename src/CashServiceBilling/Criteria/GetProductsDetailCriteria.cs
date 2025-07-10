using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class GetProductsDetailCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Age_Rpt_GetProductDetail";

    public override bool IsValid() => true;
}
