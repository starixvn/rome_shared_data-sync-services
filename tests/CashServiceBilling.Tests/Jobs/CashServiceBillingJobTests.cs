using Microsoft.Extensions.Options;
using Moq;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using CashServiceBilling.Jobs;
using CashServiceBilling.Extensions;

namespace CashServiceBilling.Tests.Jobs;

public class CashServiceBillingJobTests
{
    private readonly Mock<ILoggerService> _loggerServiceMock;
    private readonly Mock<IMaintenanceService> _maintenanceServiceMock;
    private readonly UMClientConfiguration _umClientConfigurationMock;
    private readonly Mock<ICashServiceBillingService> _cashServiceBillingServiceMock;
    private readonly Mock<IOptions<JobsSettingsModel>> _settingsMock;
    private readonly ScheduleJob[] _scheduleJobs;
    private readonly CashServiceBillingJob _job;
    private readonly JobsSettingsModel _settings;

    public CashServiceBillingJobTests()
    {
        _loggerServiceMock = new Mock<ILoggerService>();
        _maintenanceServiceMock = new Mock<IMaintenanceService>();
        _umClientConfigurationMock = new UMClientConfiguration();
        _cashServiceBillingServiceMock = new Mock<ICashServiceBillingService>();

        _settingsMock = new Mock<IOptions<JobsSettingsModel>>();
        _settings = new JobsSettingsModel
        {
        };
        _settingsMock.Setup(x => x.Value).Returns(_settings);

        _scheduleJobs = new[]
        {
                new ScheduleJob { Name = "CashServiceBillingJob" }
            };

        _job = new CashServiceBillingJob(
            _scheduleJobs,
            _loggerServiceMock.Object,
            _maintenanceServiceMock.Object,
            _umClientConfigurationMock,
            _cashServiceBillingServiceMock.Object,
            _settingsMock.Object
        );

        DateTimeExtensions.StubDateTime = new DateTime(2025, 6, 6, 23, 59, 55);
    }

    [Fact]
    public async Task ExecuteInternal_WhenNotRunningTime_ShouldWaitForNextExecution()
    {
        // Arrange
        var jobConfig = new JobConfigModel
        {
            DefaultTime = "0:10"
        };

        DateTimeExtensions.StubDateTime = new DateTime(2025, 6, 6, 0, 9, 55);
        _cashServiceBillingServiceMock.Setup(x => x.GetJobConfig())
            .Returns(jobConfig);

        // Act
        await _job.ExecuteInternal(CancellationToken.None);

        // Assert
        _cashServiceBillingServiceMock.Verify(x => x.GetJobConfig(), Times.Once);
        _loggerServiceMock.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<ScheduleJob>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteInternal_WhenRunningTime_ShouldProcessBatches()
    {
        // Arrange
        DateTimeExtensions.StubDateTime = new DateTime(2025, 6, 6, 23, 59, 55);
        var jobConfig = new JobConfigModel
        {
            DefaultTime = "0:00"
        };

        var batchList = new List<BatchModel>
            {
                new BatchModel { SiteDBGroupId = 1, AgentList = "Agent1" },
                new BatchModel { SiteDBGroupId = 2, AgentList = "Agent2" }
            };

        _cashServiceBillingServiceMock.Setup(x => x.GetJobConfig())
            .Returns(jobConfig);
        _cashServiceBillingServiceMock.Setup(x => x.GetAgentList(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(batchList);
        _cashServiceBillingServiceMock.Setup(x => x.StartJob(It.IsAny<BatchModel>()))
            .ReturnsAsync(new ResultModel { ErrCode = 0 });

        // Act
        await _job.ExecuteInternal(CancellationToken.None);

        // Assert
        _cashServiceBillingServiceMock.Verify(x => x.GetAgentList(It.IsAny<IEnumerable<int>>()), Times.Once);
        _cashServiceBillingServiceMock.Verify(x => x.StartJob(It.IsAny<BatchModel>()), Times.Exactly(2));
        _loggerServiceMock.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<ScheduleJob>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteInternal_WhenBatchProcessingFails_ShouldLogErrorAndContinue()
    {
        // Arrange
        DateTimeExtensions.StubDateTime = new DateTime(2025, 6, 6, 23, 59, 55);
        var jobConfig = new JobConfigModel
        {
            DefaultTime = "0:00"
        };

        var batchList = new List<BatchModel>
            {
                new BatchModel { SiteDBGroupId = 1, AgentList = "Agent1" }
            };

        _cashServiceBillingServiceMock.Setup(x => x.GetJobConfig())
            .Returns(jobConfig);
        _cashServiceBillingServiceMock.Setup(x => x.GetAgentList(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(batchList);
        _cashServiceBillingServiceMock.Setup(x => x.StartJob(It.IsAny<BatchModel>()))
            .ThrowsAsync(new Exception("Test error"));

        // Act
        await _job.ExecuteInternal(CancellationToken.None);

        // Assert
        _cashServiceBillingServiceMock.Verify(x => x.StartJob(It.IsAny<BatchModel>()), Times.Once);
        _loggerServiceMock.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<ScheduleJob>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteInternal_WhenJobResultHasError_ShouldNotifyError()
    {
        // Arrange
        DateTimeExtensions.StubDateTime = new DateTime(2025, 6, 6, 23, 59, 55);
        var jobConfig = new JobConfigModel
        {
            DefaultTime = "0:00"
        };

        var batchList = new List<BatchModel>
            {
                new BatchModel { SiteDBGroupId = 1, AgentList = "Agent1" }
            };

        _cashServiceBillingServiceMock.Setup(x => x.GetJobConfig())
            .Returns(jobConfig);
        _cashServiceBillingServiceMock.Setup(x => x.GetAgentList(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(batchList);
        _cashServiceBillingServiceMock.Setup(x => x.StartJob(It.IsAny<BatchModel>()))
            .ReturnsAsync(new ResultModel { ErrCode = 1, ErrMsg = "Test error" });

        // Act
        await _job.ExecuteInternal(CancellationToken.None);

        // Assert
        _cashServiceBillingServiceMock.Verify(x => x.StartJob(It.IsAny<BatchModel>()), Times.Once);
        _loggerServiceMock.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<ScheduleJob>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }
}
