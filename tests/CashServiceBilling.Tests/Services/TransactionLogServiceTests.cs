using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Moq;

namespace CashServiceBilling.Tests.Services;

public class TransactionLogServiceTests
{
    private readonly Mock<IDynamicRepository> repositoryMock;
    private readonly Mock<Fanex.Logging.ILogger> loggerMock;
    private readonly TransactionLogService service;

    public TransactionLogServiceTests()
    {
        repositoryMock = new Mock<IDynamicRepository>();
        loggerMock = new Mock<Fanex.Logging.ILogger>();
        service = new TransactionLogService(repositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task WriteLog_ShouldReturnTrue_WhenRepositoryExecutionSucceeds()
    {
        // Arrange
        var criteria = new Mock<INonQueryCommand>().Object;
        repositoryMock.Setup(x => x.ExecuteAsync(criteria)).Returns(Task.FromResult(0));

        // Act
        var result = await service.WriteLog(criteria);

        // Assert
        Assert.True(result);
        repositoryMock.Verify(x => x.ExecuteAsync(criteria), Times.Once);
        loggerMock.Verify(x => x.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }

    [Fact]
    public async Task WriteLog_ShouldReturnFalse_AndLogError_WhenRepositoryExecutionFails()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var criteria = Mock.Of<INonQueryCommand>(c => c.GetSettingKey() == "TestKey");
        repositoryMock.Setup(x => x.ExecuteAsync(criteria)).ThrowsAsync(exception);
        loggerMock.Setup(x => x.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);

        // Act
        var result = await service.WriteLog(criteria);

        // Assert
        Assert.False(result);
        repositoryMock.Verify(x => x.ExecuteAsync(criteria), Times.Once);
        loggerMock.Verify(x => x.ErrorAsync("TestKey failed.", exception), Times.Once);
    }

    [Fact]
    public async Task WriteLogs_WithCustDBGroupId_ShouldCallWriteLog_ForEachLogEntry()
    {
        // Arrange
        var logs = new List<LogEntry>
        {
            new LogEntry { UserId = 1, CustId = 2, ActionId = 3, TrackingValueMask = "mask1", TrackingValueMask2 = "mask2", TrackingValue = "val1", TrackingValue2 = "val2", CustDBGroupId = 1 },
            new LogEntry { UserId = 4, CustId = 5, ActionId = 6, TrackingValueMask = "mask3", TrackingValueMask2 = "mask4", TrackingValue = "val3", TrackingValue2 = "val4", CustDBGroupId = 1 }
        };

        const short custDBGroupId = 1;
        repositoryMock.Setup(x => x.ExecuteAsync(It.IsAny<INonQueryCommand>())).Returns(Task.FromResult(0));

        // Act
        await service.WriteLogs(logs, custDBGroupId);

        // Assert
        repositoryMock.Verify(x => x.ExecuteAsync(It.IsAny<INonQueryCommand>()), Times.Exactly(logs.Count));
    }

    [Fact]
    public async Task WriteLogs_WithCustDBGroupId_ShouldRetryWithProvidedGroupId_WhenFirstAttemptFails()
    {
        // Arrange
        var logs = new List<LogEntry>
        {
            new LogEntry { UserId = 1, CustId = 2, ActionId = 3, TrackingValueMask = "mask1", TrackingValueMask2 = "mask2", TrackingValue = "val1", TrackingValue2 = "val2", CustDBGroupId = 1 }
        };

        const short custDBGroupId = 2;

        // First attempt fails
        repositoryMock.SetupSequence(x => x.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .ThrowsAsync(new Exception("First attempt failed"))
            .Returns(Task.FromResult(0));

        loggerMock.Setup(x => x.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);

        // Act
        await service.WriteLogs(logs, custDBGroupId);

        // Assert
        repositoryMock.Verify(x => x.ExecuteAsync(It.IsAny<INonQueryCommand>()), Times.Exactly(2));
        loggerMock.Verify(x => x.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task WriteLogs_ShouldGroupByCustomerDbGroupId_AndCallWriteLogs()
    {
        // Arrange
        var logs = new List<LogEntry>
        {
            new LogEntry { UserId = 1, CustId = 2, ActionId = 3, TrackingValueMask = "mask1", TrackingValueMask2 = "mask2", TrackingValue = "val1", TrackingValue2 = "val2", CustDBGroupId = 1 },
            new LogEntry { UserId = 4, CustId = 5, ActionId = 6, TrackingValueMask = "mask3", TrackingValueMask2 = "mask4", TrackingValue = "val3", TrackingValue2 = "val4", CustDBGroupId = 1 },
            new LogEntry { UserId = 7, CustId = 8, ActionId = 9, TrackingValueMask = "mask5", TrackingValueMask2 = "mask6", TrackingValue = "val5", TrackingValue2 = "val6", CustDBGroupId = 2 }
        };

        repositoryMock.Setup(x => x.ExecuteAsync(It.IsAny<INonQueryCommand>())).Returns(Task.FromResult(0));

        // Act
        await service.WriteLogs(logs);

        // Assert
        // There should be 3 calls to ExecuteAsync (one for each log entry)
        repositoryMock.Verify(x => x.ExecuteAsync(It.IsAny<INonQueryCommand>()), Times.Exactly(3));
    }
}