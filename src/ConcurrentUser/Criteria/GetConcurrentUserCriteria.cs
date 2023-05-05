using Fanex.Data.Repository;
namespace ConcurrentUser.Criteria;

public class GetConcurrentUserCriteria : CriteriaBase
{
    public override string GetSettingKey()
        => "Age_Rpt_ConcurrentOnlineUserDetail_Sel";

    public override bool IsValid() => true;
}
