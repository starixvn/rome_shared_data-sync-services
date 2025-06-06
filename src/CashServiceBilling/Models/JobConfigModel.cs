using CashServiceBilling.Extensions;

namespace CashServiceBilling.Models;

public class JobConfigModel
{
    private const string RunningTime = "0:05";
    public string DefaultTime { get; set; } = RunningTime;

    public TimeOnly GetDefaultTime()
    {
        TimeOnly dt;
        dt = TimeOnly.TryParse(DefaultTime, out dt) ? dt : TimeOnly.Parse(RunningTime);
        return dt;
    }

    public TimeOnly GetRunTime()
    {
        var dt = DateTimeExtensions.Now;
        var currentTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

        TimeOnly rt;
        rt = TimeOnly.TryParse(DefaultTime, out rt) ? rt : TimeOnly.Parse(RunningTime);
        var todayDefaultTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, rt.Hour, rt.Minute, 0);
        return todayDefaultTime <= currentTime ? new TimeOnly(currentTime.Hour, currentTime.Minute) : rt;
    }

    public bool IsRunTime()
    {
        var currentTime = DateTimeExtensions.Now;
        var runningTime = GetRunTime();
        var runningDateTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, runningTime.Hour, runningTime.Minute, 0);
        return runningDateTime <= currentTime;
    }
}
