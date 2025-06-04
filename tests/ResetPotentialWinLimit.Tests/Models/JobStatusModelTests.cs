using ResetPotentialWinLimit.Extensions;
using ResetPotentialWinLimit.Models;
using Xunit;

namespace ResetPotentialWinLimit.Tests.Models;

public class JobStatusModelTests
{
    public JobStatusModelTests()
    {
        DateTimeExtentions.StubDateTime = new DateTime(2025, 6, 6, 10, 1, 0);
    }

    [Fact]
    public void CreateDefault_WhenCalled_ReturnsNewInstanceWithDefaultValues()
    {
        // Arrange & Act
        var result = JobStatusModel.CreateDefault();

        // Assert
        Assert.Null(result.RunningDate);
        Assert.Equal(0, result.LastMaxCustID);
        Assert.Null(result.CompletedTime);
    }

    [Fact]
    public void HasCompletedToday_WhenCompletedTimeIsNull_ReturnsFalse()
    {
        // Arrange
        var model = new JobStatusModel { CompletedTime = null };

        // Act
        var result = model.HasCompletedToday();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasCompletedToday_WhenCompletedTimeIsToday_ReturnsTrue()
    {
        // Arrange
        var model = new JobStatusModel { CompletedTime = DateTime.Now };

        // Act
        var result = model.HasCompletedToday();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasCompletedToday_WhenCompletedTimeIsYesterday_ReturnsFalse()
    {
        // Arrange
        var model = new JobStatusModel { CompletedTime = DateTimeExtentions.Now.AddDays(-1) };

        // Act
        var result = model.HasCompletedToday();

        // Assert
        Assert.False(result);
    }
}