using Fanex.Data.Repository;

namespace BalanceRecord.Criteria;

public sealed class GetJobStatusCommand : NonQueryCommand
{
    public GetJobStatusCommand(string vname)
        => VName = vname;

    public string VName { get; }

    public string VValue { get; set; } = default!;

    public override string GetSettingKey()
        => "SystemSettings_Select";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(VName);
}
