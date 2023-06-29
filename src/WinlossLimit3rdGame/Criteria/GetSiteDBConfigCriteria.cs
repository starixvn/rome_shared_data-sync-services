using Fanex.Data;
using Fanex.Data.Repository;

namespace WinlossLimit3rdGame.Criteria;

public class GetSiteDBConfigCriteria : CriteriaBase
{
    public override string GetSettingKey()
        => $"Acc_SiteDBConfigSel";    
    
    public override bool IsValid()
        => true;
}
