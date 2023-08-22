using AutoTransfer.Constants;

namespace AutoTransfer.Models;

public class JobConfigModel
{
    private const string TransferTime = "0:00";

    public string DefaultTime { get; set; } = TransferTime;
    public DateTime? ManualTime { get; set; }
    public string RunningMode { get; set; } = RunningModeConstant.Normal;
    public DateTime? CompletedTime { get; set; }

    public bool IsActive { get; set; }

    public static JobConfigModel CreateDefaultConfig()
    {
        return new JobConfigModel
        {
            DefaultTime = TransferTime,
            RunningMode = RunningModeConstant.Normal
        };
    }

    public TimeOnly GetRunningTime(DateTime currentTime)
    {
        if (ManualTime.HasValue && ManualTime.Value >= currentTime)
        {
            var manualTime = ManualTime.Value;

            if (IsSameDate(currentTime, manualTime))
            {
                return new TimeOnly(manualTime.Hour, manualTime.Minute);
            }
        }

        TimeOnly rt;
        rt = TimeOnly.TryParse(DefaultTime, out rt) ? rt : TimeOnly.Parse(TransferTime);
        var todayDefaultTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, rt.Hour, rt.Minute, 0);

        if (todayDefaultTime <= currentTime && !HasCompletedToday(currentTime))
        {
            return new TimeOnly(currentTime.Hour, currentTime.Minute);
        }

        return rt;
    }

    public string GetRunningMode(DateTime currentTime)
        => HasCompletedToday(currentTime) ? RunningMode : RunningModeConstant.Resume;
    public bool HasCompletedToday(DateTime currentTime)
        => CompletedTime.HasValue && currentTime.Date <= CompletedTime.Value.Date;

    private static bool IsSameDate(DateTime da, DateTime db)
        => da.Year == db.Year && da.Month == db.Month && da.Date == db.Date;
}
