using Shared.Configuration;
using Xunit;

namespace Shared.Tests.Configuration;

public class ScheduleJobTests
{
    [Fact]
    public void New_Always_AssignsCorrectProperties()
    {
        // Arrange
        var job = new ScheduleJob
        {
            Name = "IntegrateJob",
            Parameters = new[]
            {
                new ScheduleJobParameter { Key = "BatchSize", Value = "2000" },
                new ScheduleJobParameter { Key = "Parameter1", Value = "qwerty" }
            },
            DelayExecutionTimeInSeconds = 5,
            DelayExecutionAtExactlyTime = new TimeOnly(5, 12, 57),
            IsActive = true,
            IsTest = false
        };

        // Act & Assert
        Assert.True(
            "IntegrateJob" == job.Name
            && "BatchSize" == job.Parameters[0].Key
            && "2000" == job.Parameters[0].Value
            && "Parameter1" == job.Parameters[1].Key
            && "qwerty" == job.Parameters[1].Value
            && 5 == job.DelayExecutionTimeInSeconds
            && 5 == job.DelayExecutionAtExactlyTime.Value.Hour
            && 12 == job.DelayExecutionAtExactlyTime.Value.Minute
            && 57 == job.DelayExecutionAtExactlyTime.Value.Second
            && job.IsActive
            && !job.IsTest
        );
    }

    [Fact]
    public void JobInfo_IsTestIsTrue_ReturnsJobNameWithUAT()
    {
        // Arrange
        var job = new ScheduleJob
        {
            Name = "IntegrateJob",
            IsTest = true
        };

        // Act & Assert
        Assert.Equal("[IntegrateJob - UAT]", job.JobInfo);
    }

    [Fact]
    public void JobInfo_IsTestIsFalse_ReturnsJobNameOnly()
    {
        // Arrange
        var job = new ScheduleJob
        {
            Name = "IntegrateJob"
        };

        // Act & Assert
        Assert.Equal("[IntegrateJob]", job.JobInfo);
    }
}
