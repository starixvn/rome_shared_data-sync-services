using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Moq;

namespace CashServiceBilling.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<IDynamicRepository> mockRepository;
    private readonly Mock<ITransactionLogService> mockTransactionLogService;
    private readonly TransactionService service;

    public TransactionServiceTests()
    {
        mockRepository = new Mock<IDynamicRepository>();
        mockTransactionLogService = new Mock<ITransactionLogService>();
        service = new TransactionService(mockRepository.Object, mockTransactionLogService.Object);
    }

    [Fact]
    public async Task UpdateExpiredStatus_WhenLogsExist_ShouldWriteLogs()
    {
        // Arrange
        bool isTest = false;
        var logs = new List<LogEntry>
            {
                new LogEntry { UserId = 1 },
                new LogEntry { UserId = 2 }
            };

        mockRepository
            .Setup(r => r.FetchAsync<LogEntry>(It.Is<UpdateTransExpiredStatusCriteria>(c => c.IsTest == isTest)))
            .ReturnsAsync(logs);

        // Act
        await service.UpdateExpiredStatus(isTest);

        // Assert
        mockTransactionLogService.Verify(s => s.WriteLogs(logs, 500), Times.Once);
    }

    [Fact]
    public async Task UpdateExpiredStatus_WhenNoLogs_ShouldNotWriteLogs()
    {
        // Arrange
        bool isTest = true;
        var emptyLogs = new List<LogEntry>();

        mockRepository
            .Setup(r => r.FetchAsync<LogEntry>(It.Is<UpdateTransExpiredStatusCriteria>(c => c.IsTest == isTest)))
            .ReturnsAsync(emptyLogs);

        // Act
        await service.UpdateExpiredStatus(isTest);

        // Assert
        mockTransactionLogService.Verify(s => s.WriteLogs(It.IsAny<IEnumerable<LogEntry>>(), 500), Times.Never);
    }

    [Fact]
    public async Task UpdateExpiredStatus_ShouldUseCorrectCriteria()
    {
        // Arrange
        bool isTest = true;
        UpdateTransExpiredStatusCriteria capturedCriteria = null;

        mockRepository
            .Setup(r => r.FetchAsync<LogEntry>(It.IsAny<ICriteria>()))
            .Callback<ICriteria>(c =>
            {
                var criteria = c as UpdateTransExpiredStatusCriteria;
                capturedCriteria = criteria;
            })
            .ReturnsAsync(new List<LogEntry>());

        // Act
        await service.UpdateExpiredStatus(isTest);

        // Assert
        Assert.NotNull(capturedCriteria);
        Assert.Equal(isTest, capturedCriteria.IsTest);
    }
}