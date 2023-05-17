using Fanex.Data.Repository;

namespace BalanceRecord.Criteria;

public sealed class GetLastRowInsertedCommand : NonQueryCommand
{
    public GetLastRowInsertedCommand(short siteDBGroupId, bool isTest)
        => (SiteDBGroupId, IsTest) = (siteDBGroupId, isTest);

    public short SiteDBGroupId { get; }

    public bool IsTest { get; }

    public long? LastRowInserted { get; set; }

    public override string GetSettingKey()
        => "BalanceTriggerSiteDBConfig_Select";

    public override bool IsValid()
        => SiteDBGroupId > 0;
}
