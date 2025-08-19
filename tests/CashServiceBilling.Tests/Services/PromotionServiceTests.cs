using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Moq;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Tests.Services;

public class PromotionServiceTest
{
    private readonly Mock<IDynamicRepository> repositoryMock;
    private readonly Mock<ITransactionLogService> transactionLogServiceMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly PromotionService promotionService;
    private readonly JobsSettingsModel jobsSettings;
    private readonly ScheduleJob scheduleJob;

    public PromotionServiceTest()
    {
        repositoryMock = new Mock<IDynamicRepository>();
        transactionLogServiceMock = new Mock<ITransactionLogService>();
        loggerMock = new Mock<ILoggerService>();

        promotionService = new PromotionService(
            repositoryMock.Object,
            transactionLogServiceMock.Object,
            loggerMock.Object);

        jobsSettings = new JobsSettingsModel
        {
            IsTest = false,
            NotifyCategory = "TestCategory"
        };

        scheduleJob = new ScheduleJob();
    }

    [Fact]
    public async Task UpdateExpiredStatus_ShouldNotifyStartAndCompletion()
    {
        // Arrange
        repositoryMock.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<UpdatePromotionExpiredStatusCriteria>()))
            .ReturnsAsync(new List<LogEntry>());

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings, scheduleJob);

        // Assert
        loggerMock.Verify(l => l.Notify("[Update Promotion Expired Status] completed.", jobsSettings.NotifyCategory), Times.Once);
        loggerMock.Verify(l => l.Notify("Start [Update Promotion Expired Status].", jobsSettings.NotifyCategory), Times.Once);
    }

    [Fact]
    public async Task UpdateExpiredStatus_WhenLogsExist_ShouldWriteLogs()
    {
        // Arrange
        var logs = new List<LogEntry> { new LogEntry(), new LogEntry() };

        repositoryMock.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<UpdatePromotionExpiredStatusCriteria>()))
            .ReturnsAsync(logs);

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings, scheduleJob);

        // Assert
        transactionLogServiceMock.Verify(t => t.WriteLogs(logs, 500), Times.Once);
    }

    [Fact]
    public async Task UpdateExpiredStatus_WhenNoLogs_ShouldNotWriteLogs()
    {
        // Arrange
        repositoryMock.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<UpdatePromotionExpiredStatusCriteria>()))
            .ReturnsAsync(new List<LogEntry>());

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings, scheduleJob);

        // Assert
        transactionLogServiceMock.Verify(t => t.WriteLogs(It.IsAny<IEnumerable<LogEntry>>(), 500), Times.Never);
    }

    [Fact]
    public async Task UpdateExpiredStatus_WhenExceptionThrown_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");

        repositoryMock.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<UpdatePromotionExpiredStatusCriteria>()))
            .ThrowsAsync(exception);

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings, scheduleJob);

        // Assert
        loggerMock.Verify(l => l.Error(exception, scheduleJob, nameof(PromotionService.UpdateExpiredStatus), string.Empty), Times.Once);
    }

    [Fact]
    public async Task UpdateExpiredStatus_ShouldUseCorrectCriteriaWithTestFlag()
    {
        // Arrange
        jobsSettings.IsTest = true;

        repositoryMock.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<ICriteria>()))
            .ReturnsAsync(new List<LogEntry>())
            .Callback<ICriteria>(criteria =>
            {
                var c = criteria as UpdatePromotionExpiredStatusCriteria;
                Assert.True(c.IsTest);
            });

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings, scheduleJob);

        // Assert
        repositoryMock.Verify(r => r.FetchAsync<LogEntry>(It.Is<UpdatePromotionExpiredStatusCriteria>(c => c.IsTest == true)), Times.Once);
    }
}
