using CashServiceBilling.Jobs;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Configuration;

namespace CashServiceBilling.Tests.Jobs;

public class AccumulatedCreditJobTests
{
    private readonly Mock<IOptions<JobsSettingsModel>> _mockJobsSettings;
    private readonly Mock<IAccumulatedCreditService> _mockAccumulatedCreditService;
    private readonly JobsSettingsModel _jobsSettings;
    private readonly ScheduleJob[] _scheduleJobs;

    public AccumulatedCreditJobTests()
    {
        _jobsSettings = new JobsSettingsModel
        {
            AccumulatedCreditJobCron = "0 0 * * *"
        };

        _mockJobsSettings = new Mock<IOptions<JobsSettingsModel>>();
        _mockJobsSettings.Setup(x => x.Value).Returns(_jobsSettings);

        _mockAccumulatedCreditService = new Mock<IAccumulatedCreditService>();

        _scheduleJobs = new[]
        {
            new ScheduleJob
            {
                Name = nameof(AccumulatedCreditJob),
                IsActive = true
            }
        };
    }

    [Fact]
    public void Cron_ReturnsCorrectValue()
    {
        // Arrange
        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, _scheduleJobs, _mockAccumulatedCreditService.Object);

        // Act
        var cron = job.Cron;

        // Assert
        Assert.Equal(_jobsSettings.AccumulatedCreditJobCron, cron);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsActive_CallsAccumulateCreditToAgent()
    {
        // Arrange
        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, _scheduleJobs, _mockAccumulatedCreditService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockAccumulatedCreditService.Verify(
            x => x.AccumulateCreditToAgent(_jobsSettings, _scheduleJobs.First()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsNull_DoesNotCallAccumulateCreditToAgent()
    {
        // Arrange
        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, Array.Empty<ScheduleJob>(), _mockAccumulatedCreditService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockAccumulatedCreditService.Verify(
            x => x.AccumulateCreditToAgent(It.IsAny<JobsSettingsModel>(), It.IsAny<ScheduleJob>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsNotActive_DoesNotCallAccumulateCreditToAgent()
    {
        // Arrange
        var inactiveJobs = new[]
        {
            new ScheduleJob
            {
                Name = nameof(AccumulatedCreditJob),
                IsActive = false
            }
        };

        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, inactiveJobs, _mockAccumulatedCreditService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockAccumulatedCreditService.Verify(
            x => x.AccumulateCreditToAgent(It.IsAny<JobsSettingsModel>(), It.IsAny<ScheduleJob>()),
            Times.Never);
    }
}