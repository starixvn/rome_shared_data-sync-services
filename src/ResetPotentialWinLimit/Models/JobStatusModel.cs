namespace ResetPotentialWinLimit.Models;

public class JobStatusModel
{
    private const string RunningTime = "0:00";
    public string DefaultTime { get; set; } = RunningTime;
    public bool IsActive { get; set; }

    public DateTime? RunningDate { get; set; }
    public int LastMaxCustID { get; set; }
    public DateTime? CompletedTime { get; set; }
    public DateTime? ManualTime { get; set; }

    public static JobStatusModel CreateDefault()
    {
        return new JobStatusModel
        {
            DefaultTime = RunningTime,
            IsActive = true,
            RunningDate = null,
            LastMaxCustID = 0,
            CompletedTime = null,
            ManualTime = null
        };
    }

    private static bool IsSameDate(DateTime da, DateTime db)
        => da.Year == db.Year && da.Month == db.Month && da.Date == db.Date;

    public bool HasCompletedToday()
        => CompletedTime.HasValue && DateTime.Now.Date <= CompletedTime.Value.Date;

    public TimeOnly GetRunningTime()
    {
        var dt = DateTime.Now;
        var currentTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

        if (ManualTime.HasValue && ManualTime.Value >= currentTime)
        {
            var manualTime = ManualTime.Value;

            if (IsSameDate(currentTime, manualTime))
            {
                return new TimeOnly(manualTime.Hour, manualTime.Minute);
            }
        }

        TimeOnly rt;
        rt = TimeOnly.TryParse(DefaultTime, out rt) ? rt : TimeOnly.Parse(RunningTime);
        var todayDefaultTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, rt.Hour, rt.Minute, 0);

        if (todayDefaultTime <= currentTime && !HasCompletedToday())
        {
            return new TimeOnly(currentTime.Hour, currentTime.Minute);
        }

        return rt;
    }

    public bool IsRunTime()
    {
        var currentTime = DateTime.Now;
        var runningTime = GetRunningTime();
        return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, runningTime.Hour, runningTime.Minute, 0) <= currentTime;
    }
}
