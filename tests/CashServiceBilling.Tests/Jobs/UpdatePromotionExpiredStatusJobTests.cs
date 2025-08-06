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
    private readonly ScheduleJob[] _scheduleJobs;

    public UpdatePromotionExpiredStatusJobTests()
    {
        _jobsSettings = new JobsSettingsModel { UpdatePromotionExpiredStatusJobCron = "0 0 * * *" };
        _mockJobsSettings = new Mock<IOptions<JobsSettingsModel>>();
        _mockJobsSettings.Setup(x => x.Value).Returns(_jobsSettings);

        _mockPromotionService = new Mock<IPromotionService>();

        _scheduleJobs = new ScheduleJob[]
        {
                new ScheduleJob { Name = nameof(UpdatePromotionExpiredStatusJob), IsActive = true }
        };
    }

    [Fact]
    public void Cron_ReturnsCorrectValue()
    {
        // Arrange
        var job = new UpdatePromotionExpiredStatusJob(_mockJobsSettings.Object, _scheduleJobs, _mockPromotionService.Object);

        // Act
        var result = job.Cron;

        // Assert
        Assert.Equal(_jobsSettings.UpdatePromotionExpiredStatusJobCron, result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsActive_CallsUpdateExpiredStatus()
    {
        // Arrange
        var job = new UpdatePromotionExpiredStatusJob(_mockJobsSettings.Object, _scheduleJobs, _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(x => x.UpdateExpiredStatus(_jobsSettings), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsNotActive_DoesNotCallUpdateExpiredStatus()
    {
        // Arrange
        var inactiveScheduleJobs = new ScheduleJob[]
        {
                new ScheduleJob { Name = nameof(UpdatePromotionExpiredStatusJob), IsActive = false }
        };

        var job = new UpdatePromotionExpiredStatusJob(_mockJobsSettings.Object, inactiveScheduleJobs, _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(x => x.UpdateExpiredStatus(It.IsAny<JobsSettingsModel>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsNull_DoesNotCallUpdateExpiredStatus()
    {
        // Arrange
        var nullScheduleJobs = new ScheduleJob[]
        {
                new ScheduleJob { Name = "DifferentJobName", IsActive = true }
        };

        var job = new UpdatePromotionExpiredStatusJob(_mockJobsSettings.Object, nullScheduleJobs, _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(x => x.UpdateExpiredStatus(It.IsAny<JobsSettingsModel>()), Times.Never);
    }
}
