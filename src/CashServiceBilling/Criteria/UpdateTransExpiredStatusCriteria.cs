using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class UpdateTransExpiredStatusCriteria : CriteriaBase
{
    public UpdateTransExpiredStatusCriteria(bool isTest)
    {
        IsTest = isTest;
    }

    public bool IsTest { get; }

    public override string GetSettingKey() => "Task_CashService_UpdateExpiredStatus";

    public override bool IsValid() => true;
}