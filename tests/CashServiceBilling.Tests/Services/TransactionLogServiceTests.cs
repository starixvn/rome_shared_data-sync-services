using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Moq;

namespace CashServiceBilling.Tests.Services;

public class TransactionLogServiceTests
{
    private readonly Mock<IDynamicRepository> mockRepository;
    private readonly Mock<Fanex.Logging.ILogger> mockLogger;
    private readonly TransactionLogService service;

    public TransactionLogServiceTests()
    {
        mockRepository = new Mock<IDynamicRepository>();
        mockLogger = new Mock<Fanex.Logging.ILogger>();
        service = new TransactionLogService(mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task WriteLog_Success_ReturnsTrue()
    {
        // Arrange
        var criteria = new Mock<INonQueryCommand>().Object;
        mockRepository.Setup(r => r.ExecuteAsync(criteria)).Returns(Task.FromResult(0));

        // Act
        var result = await service.WriteLog(criteria);

        // Assert
        Assert.True(result);
        mockRepository.Verify(r => r.ExecuteAsync(criteria), Times.Once);
        mockLogger.Verify(l => l.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }

    [Fact]
    public async Task WriteLog_Exception_ReturnsFalse()
    {
        // Arrange
        var criteria = new Mock<INonQueryCommand>();
        criteria.Setup(c => c.GetSettingKey()).Returns("TestKey");

        mockRepository.Setup(r => r.ExecuteAsync(criteria.Object)).ThrowsAsync(new Exception("Test exception"));
        mockLogger.Setup(l => l.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);

        // Act
        var result = await service.WriteLog(criteria.Object);

        // Assert
        Assert.False(result);
        mockRepository.Verify(r => r.ExecuteAsync(criteria.Object), Times.Once);
        mockLogger.Verify(l => l.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task WriteLogs_WithCustomerGroupId_CallsWriteLogWithCorrectParameters()
    {
        // Arrange
        short custDBGroupId = 1;
        int batchSize = 2;
        var logs = new List<LogEntry>
            {
                new LogEntry { CustDBGroupId = custDBGroupId },
                new LogEntry { CustDBGroupId = custDBGroupId },
                new LogEntry { CustDBGroupId = custDBGroupId }
            };

        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<WriteLogRunSPCriteria>())).Returns(Task.FromResult(0));

        // Act
        await service.WriteLogs(logs, custDBGroupId, batchSize);

        // Assert
        // Should be called twice because we have 3 logs with batch size of 2 (2 batches)
        mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<WriteLogRunSPCriteria>()), Times.Exactly(2));
    }

    [Fact]
    public async Task WriteLogs_WithFailedFirstAttempt_TriesAgain()
    {
        // Arrange
        short custDBGroupId = 1;
        int batchSize = 2;
        var logs = new List<LogEntry>
            {
                new LogEntry { CustDBGroupId = custDBGroupId },
                new LogEntry { CustDBGroupId = custDBGroupId }
            };

        var attempt = 0;
        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<WriteLogRunSPCriteria>()))
            .Callback(() => attempt++)
            .Returns(() => attempt == 1
                ? throw new Exception("First attempt failed")
                : Task.FromResult(0));

        mockLogger.Setup(l => l.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);

        // Act
        await service.WriteLogs(logs, custDBGroupId, batchSize);

        // Assert
        // First attempt fails, second succeeds
        mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<WriteLogRunSPCriteria>()), Times.Exactly(2));
        mockLogger.Verify(l => l.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task WriteLogs_WithMultipleGroups_ProcessesAllGroups()
    {
        // Arrange
        var logs = new List<LogEntry>
            {
                new LogEntry { CustDBGroupId = 1 },
                new LogEntry { CustDBGroupId = 1 },
                new LogEntry { CustDBGroupId = 2 },
                new LogEntry { CustDBGroupId = 2 }
            };

        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<WriteLogRunSPCriteria>())).Returns(Task.FromResult(0));

        // Act
        await service.WriteLogs(logs, 2);

        // Assert
        // Should be called twice, once for each group
        mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<WriteLogRunSPCriteria>()), Times.Exactly(2));
    }
}
