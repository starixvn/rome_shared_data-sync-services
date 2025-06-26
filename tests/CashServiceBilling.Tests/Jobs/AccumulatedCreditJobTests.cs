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
    private readonly List<ScheduleJob> _scheduleJobs;

    public AccumulatedCreditJobTests()
    {
        _jobsSettings = new JobsSettingsModel
        {
            AccumulatedCreditJobCron = "0 0 * * *",
            IsTest = false
        };

        _mockJobsSettings = new Mock<IOptions<JobsSettingsModel>>();
        _mockJobsSettings.Setup(m => m.Value).Returns(_jobsSettings);

        _mockAccumulatedCreditService = new Mock<IAccumulatedCreditService>();

        _scheduleJobs = new List<ScheduleJob>();
    }

    [Fact]
    public void Cron_ReturnsCronExpressionFromSettings()
    {
        // Arrange
        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, _scheduleJobs.ToArray(), _mockAccumulatedCreditService.Object);

        // Act
        var result = job.Cron;

        // Assert
        Assert.Equal(_jobsSettings.AccumulatedCreditJobCron, result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsNull_DoesNothing()
    {
        // Arrange
        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, _scheduleJobs.ToArray(), _mockAccumulatedCreditService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockAccumulatedCreditService.Verify(s => s.AccumulateCreditToAgent(It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScheduleJobIsNotActive_DoesNothing()
    {
        // Arrange
        _scheduleJobs.Add(new ScheduleJob
        {
            Name = "AccumulatedCreditJob",
            IsActive = false
        });

        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, _scheduleJobs.ToArray(), _mockAccumulatedCreditService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockAccumulatedCreditService.Verify(s => s.AccumulateCreditToAgent(It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoBatchSizeParameter_UsesDefaultValue()
    {
        // Arrange
        _scheduleJobs.Add(new ScheduleJob
        {
            Name = "AccumulatedCreditJob",
            IsActive = true,
            Parameters = new[]
            {
                new ScheduleJobParameter { Key = "BatchSize" }
            }
        });

        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, _scheduleJobs.ToArray(), _mockAccumulatedCreditService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockAccumulatedCreditService.Verify(s => s.AccumulateCreditToAgent(_jobsSettings.IsTest, 500), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithBatchSizeParameter_UsesBatchSizeFromParameter()
    {
        // Arrange
        int expectedBatchSize = 1000;
        _scheduleJobs.Add(new ScheduleJob
        {
            Name = "AccumulatedCreditJob",
            IsActive = true,
            Parameters = new[]
            {
                new ScheduleJobParameter { Key = "BatchSize", Value = expectedBatchSize.ToString() }
            }
        });

        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, _scheduleJobs.ToArray(), _mockAccumulatedCreditService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockAccumulatedCreditService.Verify(s => s.AccumulateCreditToAgent(_jobsSettings.IsTest, expectedBatchSize), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidBatchSizeParameter_UsesDefaultValue()
    {
        // Arrange
        _scheduleJobs.Add(new ScheduleJob
        {
            Name = "AccumulatedCreditJob",
            IsActive = true,
            Parameters = new[]
            {
                new ScheduleJobParameter { Key = "BatchSize", Value = "invalid" }
            }
        });

        var job = new AccumulatedCreditJob(_mockJobsSettings.Object, _scheduleJobs.ToArray(), _mockAccumulatedCreditService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockAccumulatedCreditService.Verify(s => s.AccumulateCreditToAgent(_jobsSettings.IsTest, 500), Times.Once);
    }
}