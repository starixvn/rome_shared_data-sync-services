using Fanex.Data;
using Fanex.Data.Repository;

namespace WinlossLimit3rdGame.Criteria;

public sealed class InsertWinlossLimitCommand : NonQueryCommand
{
    public InsertWinlossLimitCommand(int settingStatus, string balance, bool isTest)
        => (SettingStatus, Balance, IsTest) = (settingStatus, balance, isTest);

    public int SettingStatus { get; }

    [SpParam(SqlParamName = "Banlance_Data")]
    public string Balance { get; }

    public bool IsTest { get; }

    public override string GetSettingKey()
        => "Task_Chatbot_CustWinLossReachLimitIns";

    public override bool IsValid()
        => !string.IsNullOrEmpty(Balance);
}
