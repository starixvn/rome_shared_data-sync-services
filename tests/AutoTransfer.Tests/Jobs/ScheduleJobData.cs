
using Shared.Configuration;

namespace AutoTransfer.Tests.Jobs;

public static class ScheduleJobData
{
    public static ScheduleJob[] GetData()
    {
        return new ScheduleJob[2]{
            new ScheduleJob()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
            },
            new ScheduleJob()
            {
                Name = "AutoTransferRetryJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
            }
        };
    }
}
