using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Configuration;

namespace CashServiceBilling.Jobs.Tests;

public class CalculateBonusJobTests
{
    private readonly Mock<IOptions<JobsSettingsModel>> _mockJobSettings;
    private readonly Mock<ICalculateBonusService> _mockCalculateBonusService;
    private readonly JobsSettingsModel _jobsSettings;
    private ScheduleJob[] _scheduleJobs;

    public CalculateBonusJobTests()
    {
        _jobsSettings = new JobsSettingsModel
        {
            CalculateBonusJobCron = "0 0 * * *",
            IsTest = false
        };

        _mockJobSettings = new Mock<IOptions<JobsSettingsModel>>();
        _mockJobSettings.Setup(x => x.Value).Returns(_jobsSettings);

        _mockCalculateBonusService = new Mock<ICalculateBonusService>();
    }

    [Fact]
    public void Cron_ReturnsCorrectValue()
    {
        // Arrange
        _scheduleJobs = new ScheduleJob[] { new ScheduleJob { Name = "CalculateBonusJob", IsActive = true } };
        var job = new CalculateBonusJob(_mockJobSettings.Object, _scheduleJobs, _mockCalculateBonusService.Object);

        // Act
        var result = job.Cron;

        // Assert
        Assert.Equal(_jobsSettings.CalculateBonusJobCron, result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsNull_DoesNotCallService()
    {
        // Arrange
        _scheduleJobs = Array.Empty<ScheduleJob>();
        var job = new CalculateBonusJob(_mockJobSettings.Object, _scheduleJobs, _mockCalculateBonusService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockCalculateBonusService.Verify(x => x.AddBonusTransaction(It.IsAny<bool>(), It.IsAny<ScheduleJob>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsInactive_DoesNotCallService()
    {
        // Arrange
        _scheduleJobs = new ScheduleJob[] { new ScheduleJob { Name = "CalculateBonusJob", IsActive = false } };
        var job = new CalculateBonusJob(_mockJobSettings.Object, _scheduleJobs, _mockCalculateBonusService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockCalculateBonusService.Verify(x => x.AddBonusTransaction(It.IsAny<bool>(), It.IsAny<ScheduleJob>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsActive_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var scheduleJob = new ScheduleJob { Name = "CalculateBonusJob", IsActive = true };
        _scheduleJobs = new ScheduleJob[] { scheduleJob };
        var job = new CalculateBonusJob(_mockJobSettings.Object, _scheduleJobs, _mockCalculateBonusService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockCalculateBonusService.Verify(x => x.AddBonusTransaction(_jobsSettings.IsTest, scheduleJob), Times.Once);
    }
}
