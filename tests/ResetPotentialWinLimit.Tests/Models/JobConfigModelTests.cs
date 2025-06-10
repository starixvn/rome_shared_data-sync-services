using ResetPotentialWinLimit.Models;
using ResetPotentialWinLimit.Extensions;

namespace ResetPotentialWinLimit.Tests.Models;

public class JobConfigModelTests
{
    public JobConfigModelTests()
    {
        DateTimeExtentions.StubDateTime = new DateTime(2025, 6, 6, 10, 1, 0);
    }

    [Fact]
    public void CreateDefault_WhenCalled_ReturnsDefaultConfiguration()
    {
        // Arrange & Act
        var result = JobConfigModel.CreateDefault();

        // Assert
        Assert.Equal("0:00", result.DefaultTime);
        Assert.True(result.IsActive);
        Assert.Null(result.ManualTime);
    }

    [Fact]
    public void GetRunTime_WhenManualTimeIsSetAndValid_ReturnsManualTime()
    {
        // Arrange
        var config = new JobConfigModel
        {
            ManualTime = DateTimeExtentions.Now.AddHours(1)
        };
        var jobStatus = new JobStatusModel();

        // Act
        var result = config.GetRunTime(jobStatus);

        // Assert
        Assert.Equal(config.ManualTime.Value.Hour, result.Hour);
        Assert.Equal(config.ManualTime.Value.Minute, result.Minute);
    }

    [Fact]
    public void GetRunTime_WhenManualTimeIsInPast_ReturnsDefaultTime()
    {
        // Arrange
        var config = new JobConfigModel
        {
            ManualTime = DateTimeExtentions.Now.AddHours(-1),
            DefaultTime = "14:30"
        };
        var jobStatus = new JobStatusModel();

        // Act
        var result = config.GetRunTime(jobStatus);

        // Assert
        Assert.Equal(14, result.Hour);
        Assert.Equal(30, result.Minute);
    }

    [Fact]
    public void GetRunTime_WhenDefaultTimeIsInvalid_ReturnsRunningTime()
    {
        // Arrange
        var config = new JobConfigModel
        {
            DefaultTime = "invalid-time"
        };
        var jobStatus = new JobStatusModel();

        // Act
        var result = config.GetRunTime(jobStatus);

        // Assert
        Assert.Equal(10, result.Hour);
        Assert.Equal(1, result.Minute);
    }

    [Fact]
    public void GetRunTime_WhenJobNotCompletedTodayAndDefaultTimePassed_ReturnsCurrentTime()
    {
        // Arrange
        var config = new JobConfigModel
        {
            DefaultTime = "00:00" // Set to start of day
        };
        var jobStatus = new JobStatusModel();

        // Act
        var result = config.GetRunTime(jobStatus);

        // Assert
        Assert.Equal(DateTimeExtentions.Now.Hour, result.Hour);
        Assert.Equal(DateTimeExtentions.Now.Minute, result.Minute);
    }

    [Fact]
    public void IsRunTime_WhenCurrentTimeIsAfterRunTime_ReturnsTrue()
    {
        // Arrange
        var config = new JobConfigModel
        {
            DefaultTime = "00:00" // Set to start of day
        };
        var jobStatus = new JobStatusModel();

        // Act
        var result = config.IsRunTime(jobStatus);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRunTime_WhenCurrentTimeIsBeforeRunTime_ReturnsFalse()
    {
        // Arrange
        var config = new JobConfigModel
        {
            DefaultTime = "23:59" // Set to end of day
        };
        var jobStatus = new JobStatusModel();

        // Act
        var result = config.IsRunTime(jobStatus);

        // Assert
        Assert.False(result);
    }
}