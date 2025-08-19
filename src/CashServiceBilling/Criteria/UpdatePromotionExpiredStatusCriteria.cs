using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class UpdatePromotionExpiredStatusCriteria : CriteriaBase
{
    public UpdatePromotionExpiredStatusCriteria(bool isTest)
    {
        IsTest = isTest;
    }

    public bool IsTest { get; }

    public override string GetSettingKey() => "Task_CashPromotion_UpdateExpiredStatus";

    public override bool IsValid() => true;
}