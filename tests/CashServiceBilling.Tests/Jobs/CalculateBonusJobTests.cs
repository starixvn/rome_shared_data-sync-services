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
    private readonly ScheduleJob _activeScheduleJob;

    public CalculateBonusJobTests()
    {
        _jobsSettings = new JobsSettingsModel
        {
            CalculateBonusJobCron = "0 0 * * *"
        };

        _mockJobSettings = new Mock<IOptions<JobsSettingsModel>>();
        _mockJobSettings.Setup(x => x.Value).Returns(_jobsSettings);

        _mockCalculateBonusService = new Mock<ICalculateBonusService>();

        _activeScheduleJob = new ScheduleJob
        {
            Name = nameof(CalculateBonusJob),
            IsActive = true
        };
    }

    [Fact]
    public void Cron_ReturnsExpectedValue()
    {
        // Arrange
        var job = new CalculateBonusJob(_mockJobSettings.Object, new[] { _activeScheduleJob }, _mockCalculateBonusService.Object);

        // Act
        var result = job.Cron;

        // Assert
        Assert.Equal(_jobsSettings.CalculateBonusJobCron, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveScheduleJob_CallsCalculateBonusService()
    {
        // Arrange
        var job = new CalculateBonusJob(_mockJobSettings.Object, new[] { _activeScheduleJob }, _mockCalculateBonusService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockCalculateBonusService.Verify(s => s.CalculateAndAddBonusTransactions(_jobsSettings, _activeScheduleJob), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveScheduleJob_DoesNotCallCalculateBonusService()
    {
        // Arrange
        var inactiveJob = new ScheduleJob
        {
            Name = nameof(CalculateBonusJob),
            IsActive = false
        };

        var job = new CalculateBonusJob(_mockJobSettings.Object, new[] { inactiveJob }, _mockCalculateBonusService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockCalculateBonusService.Verify(s => s.CalculateAndAddBonusTransactions(It.IsAny<JobsSettingsModel>(), It.IsAny<ScheduleJob>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullScheduleJob_DoesNotCallCalculateBonusService()
    {
        // Arrange
        var job = new CalculateBonusJob(_mockJobSettings.Object, Array.Empty<ScheduleJob>(), _mockCalculateBonusService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockCalculateBonusService.Verify(s => s.CalculateAndAddBonusTransactions(It.IsAny<JobsSettingsModel>(), It.IsAny<ScheduleJob>()), Times.Never);
    }
}
