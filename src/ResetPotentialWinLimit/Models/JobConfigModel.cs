namespace ResetPotentialWinLimit.Models;

public class JobConfigModel
{
    private const string RunningTime = "0:00";

    public bool IsActive { get; set; } = true;
    public string DefaultTime { get; set; } = RunningTime;
    public DateTime? CompletedTime { get; set; }

    public static JobConfigModel CreateDefaultConfig()
    {
        return new JobConfigModel();
    }

    public TimeOnly GetRunningTime(DateTime currentTime)
    {
        TimeOnly rt;
        rt = TimeOnly.TryParse(DefaultTime, out rt) ? rt : TimeOnly.Parse(RunningTime);
        var todayDefaultTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, rt.Hour, rt.Minute, 0);

        if (todayDefaultTime <= currentTime)
        {
            return new TimeOnly(currentTime.Hour, currentTime.Minute);
        }

        return rt;
    }
}
