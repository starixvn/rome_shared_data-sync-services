using CashServiceBilling.Jobs;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Configuration;

namespace CashServiceBilling.Tests.Jobs;

public class UpdatePromotionExpiredStatusJobTests
{
    private readonly Mock<IOptions<JobsSettingsModel>> _mockJobsSettings;
    private readonly Mock<IPromotionService> _mockPromotionService;
    private readonly JobsSettingsModel _jobsSettings;
    private readonly ScheduleJob _activeScheduleJob;
    private readonly ScheduleJob _inactiveScheduleJob;

    public UpdatePromotionExpiredStatusJobTests()
    {
        _jobsSettings = new JobsSettingsModel
        {
            UpdatePromotionExpiredStatusJobCron = "0 0 * * *"
        };

        _mockJobsSettings = new Mock<IOptions<JobsSettingsModel>>();
        _mockJobsSettings.Setup(x => x.Value).Returns(_jobsSettings);

        _mockPromotionService = new Mock<IPromotionService>();

        _activeScheduleJob = new ScheduleJob
        {
            Name = nameof(UpdatePromotionExpiredStatusJob),
            IsActive = true
        };

        _inactiveScheduleJob = new ScheduleJob
        {
            Name = nameof(UpdatePromotionExpiredStatusJob),
            IsActive = false
        };
    }

    [Fact]
    public void Cron_ReturnsCorrectValue()
    {
        // Arrange
        var job = new UpdatePromotionExpiredStatusJob(
            _mockJobsSettings.Object,
            new[] { _activeScheduleJob },
            _mockPromotionService.Object);

        // Act
        var result = job.Cron;

        // Assert
        Assert.Equal(_jobsSettings.UpdatePromotionExpiredStatusJobCron, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveJob_CallsUpdateExpiredStatus()
    {
        // Arrange
        var job = new UpdatePromotionExpiredStatusJob(
            _mockJobsSettings.Object,
            new[] { _activeScheduleJob },
            _mockPromotionService.Object);

        _mockPromotionService
            .Setup(x => x.UpdateExpiredStatus(_jobsSettings, _activeScheduleJob))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify();
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveJob_DoesNotCallUpdateExpiredStatus()
    {
        // Arrange
        var job = new UpdatePromotionExpiredStatusJob(
            _mockJobsSettings.Object,
            new[] { _inactiveScheduleJob },
            _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(x => x.UpdateExpiredStatus(It.IsAny<JobsSettingsModel>(), It.IsAny<ScheduleJob>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatchingJob_DoesNotCallUpdateExpiredStatus()
    {
        // Arrange
        var wrongNamedJob = new ScheduleJob
        {
            Name = "WrongName",
            IsActive = true
        };

        var job = new UpdatePromotionExpiredStatusJob(
            _mockJobsSettings.Object,
            new[] { wrongNamedJob },
            _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(x => x.UpdateExpiredStatus(It.IsAny<JobsSettingsModel>(), It.IsAny<ScheduleJob>()), Times.Never);
    }
}
