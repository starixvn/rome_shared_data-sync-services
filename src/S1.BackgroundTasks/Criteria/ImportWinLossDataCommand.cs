using Fanex.Data.Repository;

namespace S1.BackgroundTasks.Criteria
{
    public abstract class ImportWinLossDataBaseCommand(DateTime fromDate, DateTime toDate, string jsonData) : NonQueryCommand
    {
        public DateTime FromDate { get; } = fromDate;

        public DateTime ToDate { get; } = toDate;

        public string JsonData { get; } = jsonData;

        public int ErrCode { get; set; }

        public string ErrMessage { get; set; }

        public override bool IsValid() => FromDate <= ToDate;
    }

    public class DB052ImportWinLossDataCommand(DateTime fromDate, DateTime toDate, string jsonData) : ImportWinLossDataBaseCommand(fromDate, toDate, jsonData)
    {
        public override string GetSettingKey() => "DB052_Task_WhiteLabelWinloss_ImportData";
    }

    public class DB053ImportWinLossDataCommand(DateTime fromDate, DateTime toDate, string jsonData) : ImportWinLossDataBaseCommand(fromDate, toDate, jsonData)
    {
        public override string GetSettingKey() => "DB053_Task_WhiteLabelWinloss_ImportData";
    }
}