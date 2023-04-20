
using Shared.Configuration;

namespace AutoTransfer.Tests.Jobs;

public static class ScheduleJobData
{
    public static ScheduleJob[] GetData()
    {
        int currentHour = DateTime.Now.Hour;
        int currentMin = DateTime.Now.Minute;

        return new ScheduleJob[4]{
            new ScheduleJob()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour, currentMin, 0),
            },
            new ScheduleJob()
            {
                Name = "AutoTransferRetryJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour, currentMin, 0),
            },
            new ScheduleJob()
            {
                Name = "ResumeTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour, currentMin, 0),
            },
            new ScheduleJob()
            {
                Name = "ResumeTransferRetryJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour, currentMin, 0),
            },
        };
    }

    public static ScheduleJob[] OutOfTimeData()
    {
        int currentHour = DateTime.Now.Hour;
        int currentMin = DateTime.Now.Minute;

        return new ScheduleJob[4]{
            new ScheduleJob()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour-1, currentMin, 0),
            },
            new ScheduleJob()
            {
                Name = "AutoTransferRetryJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour-1, currentMin, 0),
            },
            new ScheduleJob()
            {
                Name = "ResumeTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour-1, currentMin, 0),
            },
            new ScheduleJob()
            {
                Name = "ResumeTransferRetryJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour-1, currentMin, 0),
            },
        };
    }
}
