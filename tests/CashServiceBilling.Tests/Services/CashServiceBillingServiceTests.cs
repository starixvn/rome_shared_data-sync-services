using Moq;
using Fanex.Data.Repository;
using Fanex.Logging;
using CashServiceBilling.Services;
using CashServiceBilling.Models;
using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Services;

public class CashServiceBillingServiceTests
{
    private readonly Mock<IDynamicRepository> _mockDynamicRepository;
    private readonly Mock<ILogger> _mockLogger;
    private readonly CashServiceBillingService _service;

    public CashServiceBillingServiceTests()
    {
        _mockDynamicRepository = new Mock<IDynamicRepository>();
        _mockLogger = new Mock<ILogger>();
        _service = new CashServiceBillingService(_mockDynamicRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public void GetJobConfig_WhenCalled_ReturnsNewJobConfigModel()
    {
        // Act
        var result = _service.GetJobConfig();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JobConfigModel>(result);
    }

    [Fact]
    public async Task GetAgentList_WhenCalled_ReturnsAgentList()
    {
        // Arrange
        var expectedAgents = new List<BatchModel>
        {
            new BatchModel { SiteDBGroupId = 1 },
            new BatchModel { SiteDBGroupId = 2 }
        };
        _mockDynamicRepository
            .Setup(repo => repo.FetchAsync<BatchModel>(It.IsAny<GetAgentListCriteria>()))
            .ReturnsAsync(expectedAgents);

        // Act
        var result = await _service.GetAgentList(new List<int> { 3 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAgents, result);
        _mockDynamicRepository.Verify(repo => repo.FetchAsync<BatchModel>(It.IsAny<GetAgentListCriteria>()), Times.Once);
    }

    [Fact]
    public async Task StartJob_WhenCalled_ProcessesLogsAndReturnsResult()
    {
        // Arrange
        var batchModel = new BatchModel { SiteDBGroupId = 1 };
        var logs = new List<LogModel>
        {
            new LogModel { Page = 1, LogData = "Test log 1" },
            new LogModel { Page = 2, LogData = "Test log 2" }
        };

        _mockDynamicRepository
            .Setup(repo => repo.FetchAsync<LogModel>(It.IsAny<StartJobCriteria>()))
            .Callback((ICriteria c) =>
            {
                var sc = c as StartJobCriteria;
                sc.ErrCode = 0; // Simulate no error
                sc.ErrMsg = "Success"; // Simulate success message
            })
            .ReturnsAsync(logs);

        _mockDynamicRepository
            .Setup(repo => repo.ExecuteAsync(It.IsAny<WriteLogCriteria>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _service.StartJob(batchModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.ErrCode);
        Assert.Equal("Success", result.ErrMsg);
        _mockDynamicRepository.Verify(repo => repo.FetchAsync<LogModel>(It.IsAny<StartJobCriteria>()), Times.Once);
        _mockDynamicRepository.Verify(repo => repo.ExecuteAsync(It.IsAny<WriteLogCriteria>()), Times.Exactly(2));
    }

    [Fact]
    public async Task WriteLog_WhenFirstAttemptFails_RetriesWithCorrectSiteDBGroupId()
    {
        // Arrange
        var siteDBGroupId = 1;
        var logModel = new LogModel { Page = 1, LogData = "Test log" };

        _mockDynamicRepository
            .SetupSequence(repo => repo.ExecuteAsync(It.IsAny<WriteLogCriteria>()))
            .ThrowsAsync(new Exception("First attempt fails"))
            .Returns(Task.FromResult(1));

        _mockLogger
            .Setup(logger => logger.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.WriteLog(siteDBGroupId, logModel);

        // Assert
        _mockDynamicRepository.Verify(repo => repo.ExecuteAsync(It.Is<WriteLogCriteria>(c => c.SiteDBGroupId == -1)), Times.Once);
        _mockDynamicRepository.Verify(repo => repo.ExecuteAsync(It.Is<WriteLogCriteria>(c => c.SiteDBGroupId == siteDBGroupId)), Times.Once);
        _mockLogger.Verify(logger => logger.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task WriteLog_WhenFirstAttemptSucceeds_DoesNotRetry()
    {
        // Arrange
        var siteDBGroupId = 1;
        var logModel = new LogModel { Page = 1, LogData = "Test log" };

        _mockDynamicRepository
            .Setup(repo => repo.ExecuteAsync(It.IsAny<WriteLogCriteria>()))
            .Returns(Task.FromResult(1));

        _mockLogger
            .Setup(logger => logger.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.WriteLog(siteDBGroupId, logModel);

        // Assert
        _mockDynamicRepository.Verify(repo => repo.ExecuteAsync(It.Is<WriteLogCriteria>(c => c.SiteDBGroupId == -1)), Times.Once);
        _mockDynamicRepository.Verify(repo => repo.ExecuteAsync(It.Is<WriteLogCriteria>(c => c.SiteDBGroupId == siteDBGroupId)), Times.Never);
        _mockLogger.Verify(logger => logger.ErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }
}