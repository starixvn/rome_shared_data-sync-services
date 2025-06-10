using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class GetJobStatusCommand : NonQueryCommand
{
    public GetJobStatusCommand(string vname)
    {
        VName = vname;
    }

    public string VName { get; }

    public string VValue { get; set; }

    public override string GetSettingKey() => "SystemSettings_Select";

    public override bool IsValid() => !string.IsNullOrEmpty(VName);
}
