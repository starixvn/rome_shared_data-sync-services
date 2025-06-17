using Microsoft.Extensions.Options;
using Moq;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using ONEbook.UM.Models;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using CashServiceBilling.Jobs;
using Xunit;

namespace CashServiceBilling.Tests.Jobs;

public class BaseCashServiceBillingJobTests
{
    private readonly ScheduleJob _mockScheduleJob;
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly Mock<IMaintenanceService> _mockMaintenanceService;
    private readonly UMClientConfiguration _mockUmClientConfiguration;
    private readonly Mock<ICashServiceBillingService> _mockCashServiceBillingService;
    private readonly Mock<IOptions<JobsSettingsModel>> _mockSettings;
    private readonly JobsSettingsModel _settingsModel;

    public BaseCashServiceBillingJobTests()
    {
        _mockScheduleJob = new ScheduleJob();
        _mockLoggerService = new Mock<ILoggerService>();
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _mockUmClientConfiguration = new UMClientConfiguration();
        _mockCashServiceBillingService = new Mock<ICashServiceBillingService>();
        _mockSettings = new Mock<IOptions<JobsSettingsModel>>();
        _settingsModel = new JobsSettingsModel { NotifyCategory = "TestCategory" };
        _mockSettings.Setup(x => x.Value).Returns(_settingsModel);
    }

    [Fact]
    public async Task WaitForSystemUM_WhenSystemIsNotUnderMaintenance_ShouldReturnImmediately()
    {
        // Arrange
        _mockMaintenanceService.Setup(x => x.GetUnderMaintenanceTimeAsync())
            .ReturnsAsync(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });
        var job = new TestCashServiceBillingJob(
            _mockScheduleJob,
            _mockLoggerService.Object,
            _mockMaintenanceService.Object,
            _mockUmClientConfiguration,
            _mockCashServiceBillingService.Object,
            _mockSettings.Object
        );

        // Act
        await job.WaitForSystemUM(CancellationToken.None);

        // Assert
        _mockMaintenanceService.Verify(x => x.GetUnderMaintenanceTimeAsync(), Times.Once);
    }

    [Fact]
    public async Task Notify_WhenCalled_ShouldLogWithCorrectFormat()
    {
        // Arrange
        const string message = "Test message";
        var job = new TestCashServiceBillingJob(
            _mockScheduleJob,
            _mockLoggerService.Object,
            _mockMaintenanceService.Object,
            _mockUmClientConfiguration,
            _mockCashServiceBillingService.Object,
            _mockSettings.Object
        );

        // Act
        await job.Notify(message);

        // Assert
        _mockLoggerService.Verify(x => x.Notify(
            It.IsAny<string>(),
            _settingsModel.NotifyCategory
        ), Times.Once);
    }
}

// Test implementation of BaseCashServiceBillingJob for testing
public class TestCashServiceBillingJob : BaseCashServiceBillingJob
{
    public TestCashServiceBillingJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        ICashServiceBillingService cashServiceBillingService,
        IOptions<JobsSettingsModel> settings)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration, cashServiceBillingService, settings)
    {
    }

    public override Task ExecuteInternal(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}