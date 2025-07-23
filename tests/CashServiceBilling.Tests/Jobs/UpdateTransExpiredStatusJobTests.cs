using CashServiceBilling.Jobs;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Microsoft.Extensions.Options;
using Moq;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Tests.Jobs;

public class UpdateTransExpiredStatusJobTests
{
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly Mock<ILoggerService> _loggerServiceMock;
    private readonly Mock<IMaintenanceService> _maintenanceServiceMock;
    private readonly ScheduleJob[] _scheduleJobs;
    private readonly UMClientConfiguration _umClientConfiguration;
    private readonly Mock<IOptions<JobsSettingsModel>> _settingsMock;
    private readonly JobsSettingsModel _settings;

    public UpdateTransExpiredStatusJobTests()
    {
        _transactionServiceMock = new Mock<ITransactionService>();
        _loggerServiceMock = new Mock<ILoggerService>();
        _maintenanceServiceMock = new Mock<IMaintenanceService>();

        // Setup schedule job
        _scheduleJobs = new[]
        {
                new ScheduleJob
                {
                    Name = nameof(UpdateTransExpiredStatusJob),
                    DelayExecutionTimeInSeconds = 5
                }
            };

        _umClientConfiguration = new UMClientConfiguration();

        _settings = new JobsSettingsModel { IsTest = false };
        _settingsMock = new Mock<IOptions<JobsSettingsModel>>();
        _settingsMock.Setup(x => x.Value).Returns(_settings);
    }

    [Fact]
    public async Task ExecuteInternal_CallsUpdateExpiredStatus()
    {
        // Arrange
        _maintenanceServiceMock.Setup(x => x.IsUnderMaintenanceAsync()).ReturnsAsync(false);

        var job = new UpdateTransExpiredStatusJob(
            _scheduleJobs,
            _loggerServiceMock.Object,
            _maintenanceServiceMock.Object,
            _umClientConfiguration,
            _transactionServiceMock.Object,
            _settingsMock.Object
        );

        // Act
        await job.ExecuteInternal(CancellationToken.None);

        // Assert
        _transactionServiceMock.Verify(x => x.UpdateExpiredStatus(_settings.IsTest), Times.Once);
    }

    [Fact]
    public async Task ExecuteInternal_WithException_LogsError()
    {
        // Arrange
        _maintenanceServiceMock.Setup(x => x.IsUnderMaintenanceAsync()).ReturnsAsync(false);

        var expectedException = new Exception("Test exception");
        _transactionServiceMock.Setup(x => x.UpdateExpiredStatus(It.IsAny<bool>()))
            .ThrowsAsync(expectedException);

        var job = new UpdateTransExpiredStatusJob(
            _scheduleJobs,
            _loggerServiceMock.Object,
            _maintenanceServiceMock.Object,
            _umClientConfiguration,
            _transactionServiceMock.Object,
            _settingsMock.Object
        );

        // Act
        await job.ExecuteInternal(CancellationToken.None);

        // Assert
        _loggerServiceMock.Verify(x =>
            x.Error(
                It.Is<Exception>(o => o == expectedException),
                It.IsAny<ScheduleJob>(),
                It.IsAny<string>(),
                string.Empty
            ),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithNoMatchingScheduleJob_CreatesDefaultJob()
    {
        // Arrange
        var emptyScheduleJobs = Array.Empty<ScheduleJob>();

        // Act
        var job = new UpdateTransExpiredStatusJob(
            emptyScheduleJobs,
            _loggerServiceMock.Object,
            _maintenanceServiceMock.Object,
            _umClientConfiguration,
            _transactionServiceMock.Object,
            _settingsMock.Object
        );

        // Assert
        Assert.NotNull(job);
        // The job should be created with a new default ScheduleJob
    }
}
