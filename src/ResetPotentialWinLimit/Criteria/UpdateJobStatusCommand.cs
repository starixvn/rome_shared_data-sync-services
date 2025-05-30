using Fanex.Data.Repository;

namespace ResetPotentialWinLimit.Criteria;

public class UpdateJobStatusCommand : NonQueryCommand
{
    public UpdateJobStatusCommand(string vname, string vvalue)
    {
        VName = vname;
        VValue = vvalue;
    }

    public string VName { get; }

    public string VValue { get; set; } = default!;

    public override string GetSettingKey() => "SystemSettings_Update";

    public override bool IsValid() => !string.IsNullOrEmpty(VName);
}
