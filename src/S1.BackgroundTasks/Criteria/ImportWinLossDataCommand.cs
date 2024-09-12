using Fanex.Data.Repository;

namespace S1.BackgroundTasks.Criteria
{
    public class ImportWinLossDataCommand(DateTime fromDate, DateTime toDate, string jsonData) : NonQueryCommand
    {
        public DateTime FromDate { get; } = fromDate;

        public DateTime ToDate { get; } = toDate;

        public string JsonData { get; } = jsonData;

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public override string GetSettingKey() => "Task_WhiteLabelWinloss_ImportData";

        public override bool IsValid() => FromDate <= ToDate && !string.IsNullOrWhiteSpace(JsonData);
    }
}