using CashServiceBilling.Models;
using Xunit;
using System;
using CashServiceBilling.Extensions;

namespace CashServiceBilling.Tests.Models;

public class JobConfigModelTests
{
    private readonly JobConfigModel _jobConfigModel;

    public JobConfigModelTests()
    {
        DateTimeExtensions.StubDateTime = new DateTime(2024, 1, 1, 0, 0, 0); // Set a fixed date for testing
        _jobConfigModel = new JobConfigModel();
    }

    [Fact]
    public void GetRunTime_WhenCurrentTimeIsBeforeDefaultTime_ReturnsDefaultTime()
    {
        // Arrange
        var defaultTime = TimeOnly.Parse("0:05");
        _jobConfigModel.DefaultTime = defaultTime.ToString("HH:mm");

        // Act
        var result = _jobConfigModel.GetRunTime();

        // Assert
        Assert.Equal(defaultTime, result);
    }

    [Fact]
    public void GetRunTime_WhenCurrentTimeIsAfterDefaultTime_ReturnsCurrentTime()
    {
        // Arrange
        var defaultTime = TimeOnly.Parse("0:05");
        _jobConfigModel.DefaultTime = defaultTime.ToString("HH:mm");
        DateTimeExtensions.StubDateTime = new DateTime(2024, 1, 1, 0, 10, 0); // Set a fixed date for testing

        // Act
        var result = _jobConfigModel.GetRunTime();

        // Assert
        Assert.Equal(new TimeOnly(DateTimeExtensions.Now.Hour, DateTimeExtensions.Now.Minute), result);
    }

    [Fact]
    public void GetRunTime_WhenDefaultTimeIsInvalid_ReturnsRunningTime()
    {
        // Arrange
        _jobConfigModel.DefaultTime = "invalid_time";

        // Act
        var result = _jobConfigModel.GetRunTime();

        // Assert
        Assert.Equal(TimeOnly.Parse("0:05"), result);
    }

    [Fact]
    public void IsRunTime_WhenCurrentTimeIsBeforeRunningTime_ReturnsFalse()
    {
        // Arrange
        _jobConfigModel.DefaultTime = "0:05";
        DateTimeExtensions.StubDateTime = new DateTime(2024, 1, 1, 0, 0, 0); // Set a fixed date for testing

        // Act
        var result = _jobConfigModel.IsRunTime();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRunTime_WhenCurrentTimeIsAfterRunningTime_ReturnsTrue()
    {
        // Arrange
        _jobConfigModel.DefaultTime = "0:00";
        DateTimeExtensions.StubDateTime = new DateTime(2024, 1, 1, 0, 10, 0); // Set a fixed date for testing

        // Act
        var result = _jobConfigModel.IsRunTime();

        // Assert
        Assert.True(result);
    }
}