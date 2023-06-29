using Fanex.Data;
using Fanex.Data.Repository;

namespace WinlossLimit3rdGame.Criteria;

public sealed class GetWinlossLimitCommand : NonQueryCommand
{
    public GetWinlossLimitCommand(string dbName)
    => DBName = dbName;

    private string DBName { get; set; }

    [SpParam(SqlParamName = "Banlance_Data")]
    public string Balance { get; set; }

    public override string GetSettingKey()
        => $"{DBName}_Task_Chatbot_CustBalanceGameSel";

    public override bool IsValid()
    => !string.IsNullOrEmpty(DBName);
}
