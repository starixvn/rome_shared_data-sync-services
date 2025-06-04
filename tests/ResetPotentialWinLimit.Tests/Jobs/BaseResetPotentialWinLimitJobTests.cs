using Microsoft.Extensions.Options;
using Moq;
using ONEbook.UM.Configurations;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using ResetPotentialWinLimit.Jobs;
using ResetPotentialWinLimit.Models;
using ResetPotentialWinLimit.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;
using Xunit;

namespace ResetPotentialWinLimit.Tests.Jobs;

public class BaseResetPotentialWinLimitJobTests
{
    private readonly ScheduleJob _mockScheduleJob;
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly UMClientConfiguration _mockUmClientConfiguration;
    private readonly Mock<IResetPotentialWinLimitService> _mockResetPotentialWinLimitService;
    private readonly Mock<IOptions<JobsSettingsModel>> _mockSettings;
    private readonly JobsSettingsModel _settings;
    private readonly TestResetPotentialWinLimitJob _job;

    public BaseResetPotentialWinLimitJobTests()
    {
        _mockScheduleJob = new ScheduleJob();
        _mockLoggerService = new Mock<ILoggerService>();
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _mockUmClientConfiguration = new UMClientConfiguration();
        _mockResetPotentialWinLimitService = new Mock<IResetPotentialWinLimitService>();
        _settings = new JobsSettingsModel();
        _mockSettings = new Mock<IOptions<JobsSettingsModel>>();
        _mockSettings.Setup(x => x.Value).Returns(_settings);

        _job = new TestResetPotentialWinLimitJob(
            _mockScheduleJob,
            _mockLoggerService.Object,
            _mockMaintenanceService.Object,
            _mockUmClientConfiguration,
            _mockResetPotentialWinLimitService.Object,
            _mockSettings.Object
        );
    }

    [Fact]
    public void IsRunTime_WhenTimeMatches_ShouldReturnTrue()
    {
        // Arrange
        var runningTime = new TimeOnly(14, 30);
        var currentTime = new DateTime(2024, 1, 1, 14, 30, 0);

        // Act
        var result = _job.IsRunTime(runningTime, currentTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRunTime_WhenTimeDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        var runningTime = new TimeOnly(14, 30);
        var currentTime = new DateTime(2024, 1, 1, 15, 30, 0);

        // Act
        var result = _job.IsRunTime(runningTime, currentTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Notify_WhenSiteDBNameIsEmpty_ShouldLogWithoutSiteDBName()
    {
        // Arrange
        _settings.SiteDBName = string.Empty;
        var message = "Test message";

        // Act
        await _job.Notify(message);

        // Assert
        _mockLoggerService.Verify(x => x.Notify(
            It.Is<string>(s => s == $"{_mockScheduleJob.JobInfo} {message}"),
            It.Is<string>(s => s == _settings.NotifyCategory)), Times.Once);
    }

    [Fact]
    public async Task Notify_WhenSiteDBNameIsProvided_ShouldLogWithSiteDBName()
    {
        // Arrange
        _settings.SiteDBName = "TestDB";
        var message = "Test message";

        // Act
        await _job.Notify(message);

        // Assert
        _mockLoggerService.Verify(x => x.Notify(
            It.Is<string>(s => s == $"{_mockScheduleJob.JobInfo} - [TestDB] {message}"),
            It.Is<string>(s => s == _settings.NotifyCategory)), Times.Once);
    }

    [Fact]
    public async Task WaitForSystemUM_WhenSystemIsNotUnderMaintenance_ShouldProceedImmediately()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTimeAsync())
            .ReturnsAsync(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });

        // Act
        await _job.WaitForSystemUM(cancellationToken);

        // Assert
        _mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTimeAsync(), Times.Once);
    }

    [Fact]
    public async Task WaitForSystemUM_WhenSystemBecomesAvailable_ShouldProceed()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var callCount = 0;
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTimeAsync())
            .ReturnsAsync(() =>
            {
                callCount++;
                return new UnderMaintenanceTime
                {
                    IsUnderMaintenanceTime = callCount == 1,
                    From = callCount == 1 ? "2024-01-01 10:00:00" : null,
                    To = callCount == 1 ? "2024-01-01 11:00:00" : null
                };
            });

        // Act
        await _job.WaitForSystemUM(cancellationToken);

        // Assert
        _mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTimeAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task WaitForSystemUM_WhenMaintenanceServiceThrowsException_ShouldLogErrorAndContinue()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTimeAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await _job.WaitForSystemUM(cancellationToken);

        // Assert
        _mockLoggerService.Verify(x => x.Error(
            It.IsAny<Exception>(),
            It.IsAny<ScheduleJob>(),
            It.IsAny<string>(),
            "Could not get UM information."), Times.Once);
    }
}

// Test implementation of BaseResetPotentialWinLimitJob
public class TestResetPotentialWinLimitJob : BaseResetPotentialWinLimitJob
{
    public TestResetPotentialWinLimitJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IResetPotentialWinLimitService resetPotentialWinLimitService,
        IOptions<JobsSettingsModel> settings)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration, resetPotentialWinLimitService, settings)
    {
    }

    public override Task ExecuteInternal(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}