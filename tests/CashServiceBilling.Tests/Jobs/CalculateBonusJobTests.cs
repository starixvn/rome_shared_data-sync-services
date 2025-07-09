using CashServiceBilling.Jobs;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Configuration;

namespace CashServiceBilling.Tests.Jobs;

public class CalculateBonusJobTests
{
    private readonly Mock<IOptions<JobsSettingsModel>> _mockJobsSettings;
    private readonly Mock<ICalculateBonusService> _mockPromotionService;
    private readonly ScheduleJob[] _scheduleJobs;
    private readonly JobsSettingsModel _jobsSettingsModel;

    public CalculateBonusJobTests()
    {
        _mockJobsSettings = new Mock<IOptions<JobsSettingsModel>>();
        _mockPromotionService = new Mock<ICalculateBonusService>();
        _jobsSettingsModel = new JobsSettingsModel
        {
            CalculateBonusJobCron = "0 0 * * *",
            IsTest = false
        };
        _mockJobsSettings.Setup(x => x.Value).Returns(_jobsSettingsModel);
        _scheduleJobs = new[]
        {
                new ScheduleJob
                {
                    Name = "CalculateBonusJob",
                    IsActive = true,
                    Parameters = new[]
                    {
                        new ScheduleJobParameter { Key = "BatchSize", Value = "1000" }
                    }
                }
            };
    }

    [Fact]
    public void Cron_ReturnsCorrectValue()
    {
        // Arrange
        var job = new CalculateBonusJob(_mockJobsSettings.Object, _scheduleJobs, _mockPromotionService.Object);

        // Act
        var cron = job.Cron;

        // Assert
        Assert.Equal(_jobsSettingsModel.CalculateBonusJobCron, cron);
    }

    [Fact]
    public async Task ExecuteAsync_JobIsActive_CallsAddPendingPromotionWithCorrectParameters()
    {
        // Arrange
        var job = new CalculateBonusJob(_mockJobsSettings.Object, _scheduleJobs, _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(s => s.AddBonusTransaction(false, 1000), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_JobIsNotActive_DoesNotCallAddPendingPromotion()
    {
        // Arrange
        var inactiveScheduleJobs = new[]
        {
                new ScheduleJob
                {
                    Name = "CalculateBonusJob",
                    IsActive = false,
                    Parameters = new[]
                    {
                        new ScheduleJobParameter { Key = "BatchSize", Value = "1000" }
                    }
                }
            };
        var job = new CalculateBonusJob(_mockJobsSettings.Object, inactiveScheduleJobs, _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(s => s.AddBonusTransaction(It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_JobNotFound_DoesNotCallAddPendingPromotion()
    {
        // Arrange
        var emptyScheduleJobs = Array.Empty<ScheduleJob>();
        var job = new CalculateBonusJob(_mockJobsSettings.Object, emptyScheduleJobs, _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(s => s.AddBonusTransaction(It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_NoBatchSizeParameter_UsesDefaultBatchSize()
    {
        // Arrange
        var scheduleJobsWithoutBatchSize = new[]
        {
                new ScheduleJob
                {
                    Name = "CalculateBonusJob",
                    IsActive = true,
                    Parameters = Array.Empty<ScheduleJobParameter>()
                }
            };
        var job = new CalculateBonusJob(_mockJobsSettings.Object, scheduleJobsWithoutBatchSize, _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(s => s.AddBonusTransaction(false, 3000), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidBatchSizeParameter_UsesDefaultBatchSize()
    {
        // Arrange
        var scheduleJobsWithInvalidBatchSize = new[]
        {
                new ScheduleJob
                {
                    Name = "CalculateBonusJob",
                    IsActive = true,
                    Parameters = new[]
                    {
                        new ScheduleJobParameter { Key = "BatchSize", Value = "invalid" }
                    }
                }
            };
        var job = new CalculateBonusJob(_mockJobsSettings.Object, scheduleJobsWithInvalidBatchSize, _mockPromotionService.Object);

        // Act
        await job.ExecuteAsync();

        // Assert
        _mockPromotionService.Verify(s => s.AddBonusTransaction(false, 3000), Times.Once);
    }
}
