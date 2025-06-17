using Fanex.Data.Repository;

namespace CashServiceBilling.Criteria;

public class GetAgentListCriteria : CriteriaBase
{
    public GetAgentListCriteria(IEnumerable<int> siteDBGroupIds)
    {
        SiteDBGroupIds = string.Join(",", siteDBGroupIds);
    }

    public string SiteDBGroupIds { get; }

    public override string GetSettingKey() => "Age_CashServiceBilling_AgentSel";

    public override bool IsValid() => true;
}
