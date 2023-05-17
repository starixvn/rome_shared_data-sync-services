using Fanex.Data.Repository;

namespace BalanceRecord.Criteria;

public sealed class InsertBalanceRecordFromSiteDBCommand : NonQueryCommand
{
    public InsertBalanceRecordFromSiteDBCommand(
        short siteDBGroupId,
        byte[] transInfo,
        bool isTest)
        => (SiteDBGroupId, TransInfo, IsTest) = (siteDBGroupId, transInfo, isTest);

    public short SiteDBGroupId { get; }

    public byte[] TransInfo { get; }

    public bool IsTest { get; }

    public string ErrorMsg { get; set; } = default!;

    public override string GetSettingKey()
        => "BalanceTriggerSiteDB_Insert";

    public override bool IsValid()
        => SiteDBGroupId > 0;
}
