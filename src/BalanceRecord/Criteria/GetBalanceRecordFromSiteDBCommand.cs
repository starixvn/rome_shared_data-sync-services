using Fanex.Data;
using Fanex.Data.Repository;

namespace BalanceRecord.Criteria;

public sealed class GetBalanceRecordFromSiteDBCommand : NonQueryCommand
{
    public GetBalanceRecordFromSiteDBCommand(
        string siteDBName,
        long lastRowInserted,
        int batch,
        bool isTest)
        => (SiteDBName, LastRowInserted, Batch, IsTest) = (siteDBName, lastRowInserted, batch, isTest);

    [SpParam(Ignored = true)]
    public string SiteDBName { get; }

    public long LastRowInserted { get; }

    public int Batch { get; }

    public bool IsTest { get; }

    public byte[] TransInfo { get; set; } = default!;

    public override string GetSettingKey()
        => $"SiteDB_BalanceTrigger_Select_{SiteDBName}";

    public override bool IsValid()
        => LastRowInserted >= 0 && Batch > 0;
}
