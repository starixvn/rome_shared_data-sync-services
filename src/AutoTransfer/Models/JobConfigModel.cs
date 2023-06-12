using AutoTransfer.Constants;

namespace AutoTransfer.Models;

public class JobConfigModel
{
    private const string TransferTime = "0:00";

    public string DefaultTime { get; set; } = TransferTime;
    public DateTime? ManualTime { get; set; }
    public string RunningMode { get; set; } = RunningModeConstant.Normal;

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
            if (manualTime.Year == currentTime.Year && manualTime.Month == currentTime.Month && manualTime.Date == currentTime.Date)
            {
                return new TimeOnly(manualTime.Hour, manualTime.Minute);
            }
        }

        if (TimeOnly.TryParse(DefaultTime, out TimeOnly rt))
        {
            return rt;
        }

        rt = TimeOnly.Parse(TransferTime);
        return rt;
    }
}
