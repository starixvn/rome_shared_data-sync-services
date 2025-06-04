using ResetPotentialWinLimit.Extensions;

namespace ResetPotentialWinLimit.Models;

public class JobConfigModel
{
    private const string RunningTime = "0:00";
    public string DefaultTime { get; set; } = RunningTime;
    public bool IsActive { get; set; }
    public DateTime? ManualTime { get; set; }

    public static JobConfigModel CreateDefault()
    {
        return new JobConfigModel
        {
            DefaultTime = RunningTime,
            IsActive = true,
            ManualTime = null
        };
    }

    public TimeOnly GetDefaultTime()
    {
        TimeOnly rt;
        rt = TimeOnly.TryParse(DefaultTime, out rt) ? rt : TimeOnly.Parse(RunningTime);
        return rt;
    }

    public TimeOnly GetRunTime(JobStatusModel jobStatus)
    {
        var dt = DateTimeExtentions.Now;
        var currentTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

        if (ManualTime.HasValue && ManualTime.Value >= currentTime)
        {
            var manualTime = ManualTime.Value;

            if (DateTimeExtentions.IsSameDate(currentTime, manualTime))
            {
                return new TimeOnly(manualTime.Hour, manualTime.Minute);
            }
        }

        TimeOnly rt;
        rt = TimeOnly.TryParse(DefaultTime, out rt) ? rt : TimeOnly.Parse(RunningTime);
        var todayDefaultTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, rt.Hour, rt.Minute, 0);

        if (todayDefaultTime <= currentTime && !jobStatus.HasCompletedToday())
        {
            return new TimeOnly(currentTime.Hour, currentTime.Minute);
        }

        return rt;
    }

    public bool IsRunTime(JobStatusModel jobStatus)
    {
        var currentTime = DateTimeExtentions.Now;
        var runningTime = GetRunTime(jobStatus);
        return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, runningTime.Hour, runningTime.Minute, 0) <= currentTime;
    }
}
