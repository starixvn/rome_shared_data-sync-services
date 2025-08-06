using System.ComponentModel.DataAnnotations;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Moq;
using Shared.Services;

namespace CashServiceBilling.Tests.Services;

public class PromotionServiceTests
{
    private readonly Mock<IDynamicRepository> mockRepository;
    private readonly Mock<ITransactionLogService> mockTransactionLogService;
    private readonly Mock<ILoggerService> mockLogger;
    private readonly PromotionService promotionService;
    private readonly JobsSettingsModel jobsSettings;

    public PromotionServiceTests()
    {
        mockRepository = new Mock<IDynamicRepository>();
        mockTransactionLogService = new Mock<ITransactionLogService>();
        mockLogger = new Mock<ILoggerService>();

        promotionService = new PromotionService(
            mockRepository.Object,
            mockTransactionLogService.Object,
            mockLogger.Object);

        jobsSettings = new JobsSettingsModel
        {
            IsTest = false,
            NotifyCategory = "TestCategory"
        };
    }

    [Fact]
    public async Task UpdateExpiredStatus_ShouldNotifyStartAndCompletion()
    {
        // Arrange
        mockRepository.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<UpdatePromotionExpiredStatusCriteria>()))
            .ReturnsAsync(new List<LogEntry>());

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings);

        // Assert
        mockLogger.Verify(l => l.Notify("Start [Update Promotion Expired Status].", jobsSettings.NotifyCategory), Times.Once);
        mockLogger.Verify(l => l.Notify("[Update Promotion Expired Status] completed.", jobsSettings.NotifyCategory), Times.Once);
    }

    [Fact]
    public async Task UpdateExpiredStatus_WhenLogsExist_ShouldWriteLogs()
    {
        // Arrange
        var logs = new List<LogEntry> { new LogEntry(), new LogEntry() };
        mockRepository.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<UpdatePromotionExpiredStatusCriteria>()))
            .ReturnsAsync(logs);

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings);

        // Assert
        mockTransactionLogService.Verify(t => t.WriteLogs(logs), Times.Once);
    }

    [Fact]
    public async Task UpdateExpiredStatus_WhenNoLogs_ShouldNotWriteLogs()
    {
        // Arrange
        mockRepository.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<UpdatePromotionExpiredStatusCriteria>()))
            .ReturnsAsync(new List<LogEntry>());

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings);

        // Assert
        mockTransactionLogService.Verify(t => t.WriteLogs(It.IsAny<IEnumerable<LogEntry>>()), Times.Never);
    }

    [Fact]
    public async Task UpdateExpiredStatus_ShouldPassCorrectIsTestValueToCriteria()
    {
        // Arrange
        jobsSettings.IsTest = true;
        mockRepository.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<ICriteria>()))
            .ReturnsAsync(new List<LogEntry>())
            .Callback<ICriteria>(criteria =>
            {
                var c = criteria as UpdatePromotionExpiredStatusCriteria;
                Assert.Equal(jobsSettings.IsTest, c.IsTest);
            });

        // Act
        await promotionService.UpdateExpiredStatus(jobsSettings);

        // Assert
        mockRepository.Verify(r => r.FetchAsync<LogEntry>(It.IsAny<UpdatePromotionExpiredStatusCriteria>()), Times.Once);
    }
}
