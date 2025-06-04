using ResetPotentialWinLimit.Extensions;

namespace ResetPotentialWinLimit.Models;

public class JobStatusModel
{
    public DateTime? RunningDate { get; set; }
    public int LastMaxCustID { get; set; }
    public DateTime? CompletedTime { get; set; }

    public static JobStatusModel CreateDefault()
    {
        return new JobStatusModel
        {
            RunningDate = null,
            LastMaxCustID = 0,
            CompletedTime = null
        };
    }

    public bool HasCompletedToday()
        => CompletedTime.HasValue && DateTimeExtentions.Now.Date <= CompletedTime.Value.Date;
}
