using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Moq;
using Shared.Configuration;
using Shared.Services;

namespace CashServiceBilling.Tests.Services;

public class AccumulatedCreditServiceTests
{
    private readonly Mock<IDynamicRepository> _repositoryMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<ITransactionLogService> _transactionLogServiceMock;
    private readonly AccumulatedCreditService _service;

    public AccumulatedCreditServiceTests()
    {
        _repositoryMock = new Mock<IDynamicRepository>();
        _loggerMock = new Mock<ILoggerService>();
        _transactionLogServiceMock = new Mock<ITransactionLogService>();
        _service = new AccumulatedCreditService(_repositoryMock.Object, _loggerMock.Object, _transactionLogServiceMock.Object);
    }

    [Fact]
    public async Task AccumulateCreditToAgent_WithNoWinLoss_LogsCorrectCounts()
    {
        // Arrange
        var jobSettings = new JobsSettingsModel { NotifyCategory = "TestCategory", IsTest = false };
        var scheduleJob = new ScheduleJob();

        _repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(criteria =>
            {
                var c = criteria as GetCashMemberListCriteria;
                c.CustIdList = "";
            });

        // Act
        await _service.AccumulateCreditToAgent(jobSettings, scheduleJob);

        // Assert
        _loggerMock.Verify(l => l.Notify(It.Is<string>(s => s.Contains("Cash Member Yesterday WinLoss Count: 0")), jobSettings.NotifyCategory), Times.Once);
        _loggerMock.Verify(l => l.Notify(It.Is<string>(s => s.Contains("completed")), jobSettings.NotifyCategory), Times.Once);
    }

    [Fact]
    public async Task AccumulateCreditToAgent_WithWinLossData_ProcessesData()
    {
        // Arrange
        var jobSettings = new JobsSettingsModel { NotifyCategory = "TestCategory", IsTest = false };
        var scheduleJob = new ScheduleJob { Parameters = new[] { new ScheduleJobParameter { Key = "BatchSize", Value = "200" } } };

        var winlossData = new[]
        {
                new MemberYesterdayWL { CustId = 1 },
                new MemberYesterdayWL { CustId = 2 }
            };

        _repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(criteria =>
            {
                if (criteria is GetCashMemberListCriteria getCashMemberListCriteria)
                {
                    getCashMemberListCriteria.CustIdList = JsonSerializer.Serialize(winlossData);
                }
                else if (criteria is SyncCashAccumCreditCriteria getSyncCashAccumCreditCriteria)
                {
                    getSyncCashAccumCreditCriteria.ErrCode = 0;
                    getSyncCashAccumCreditCriteria.ErrMsg = "";
                }
            });

        var cashAccumData = new[]
        {
            new CashAccumCredit { CustDBGroupId = 1, CustId = 1 },
            new CashAccumCredit { CustDBGroupId = 1, CustId = 2 }
        };

        _repositoryMock.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<ICriteria>()))
            .Callback<ICriteria>(criteria =>
            {
                var c = criteria as AddManualDepositTransCriteria;
                c.CashAccumCreditTbl = JsonSerializer.Serialize(cashAccumData);
            })
            .ReturnsAsync(new List<LogEntry>());

        // Act
        await _service.AccumulateCreditToAgent(jobSettings, scheduleJob);

        // Assert
        _loggerMock.Verify(l => l.Notify(It.Is<string>(s => s.Contains("Cash Member Yesterday WinLoss Count: 2")), jobSettings.NotifyCategory), Times.Once);
        _repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<SyncCashAccumCreditCriteria>()), Times.Once);
    }

    [Fact]
    public async Task GetMemberYesterdayWinLoss_ReturnsExpectedData()
    {
        // Arrange
        var expectedJson = "[{\"CustId\":1}]";
        _repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(criteria =>
            {
                var c = criteria as GetCashMemberListCriteria;
                c.CustIdList = expectedJson;
            });

        // Act
        var result = await _service.GetMemberYesterdayWinLoss(false);

        // Assert
        Assert.Equal(expectedJson, result);
        _repositoryMock.Verify(r => r.ExecuteAsync(It.Is<GetCashMemberListCriteria>(c => !c.IsTest)), Times.Once);
    }

    [Fact]
    public async Task ProcessMemberYesterdayWinLoss_CallsExpectedDependencies()
    {
        // Arrange
        var memberWinLoss = new[] { new MemberYesterdayWL { CustId = 1 } };
        var cashAccumData = new[] { new CashAccumCredit { CustDBGroupId = 1, CustId = 1 } };
        var logs = new List<LogEntry> { new LogEntry() };

        _repositoryMock.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<ICriteria>()))
            .Callback<ICriteria>(criteria =>
            {
                var c = criteria as AddManualDepositTransCriteria;
                c.CashAccumCreditTbl = JsonSerializer.Serialize(cashAccumData);
            })
            .ReturnsAsync(logs);

        _repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(criteria =>
            {
                var c = criteria as SyncCashAccumCreditCriteria;
                c.ErrCode = 0;
                c.ErrMsg = "";
            });

        // Act
        await _service.ProcessMemberYesterdayWinLoss(memberWinLoss);

        // Assert
        _repositoryMock.Verify(r => r.FetchAsync<LogEntry>(It.IsAny<AddManualDepositTransCriteria>()), Times.Once);
        _repositoryMock.Verify(r => r.ExecuteAsync(It.IsAny<SyncCashAccumCreditCriteria>()), Times.Once);
        _transactionLogServiceMock.Verify(t => t.WriteLogs(logs), Times.Once);
    }

    [Fact]
    public async Task SyncCashAccumCredit_WithError_LogsError()
    {
        // Arrange
        var cashAccumData = JsonSerializer.Serialize(new[] { new CashAccumCredit { CustId = 1 } });
        const short groupId = 1;
        const int errCode = 500;
        const string errMsg = "Error message";

        _repositoryMock.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(criteria =>
            {
                var c = criteria as SyncCashAccumCreditCriteria;
                c.ErrCode = errCode;
                c.ErrMsg = errMsg;
            });

        // Act
        var result = await _service.SyncCashAccumCredit(cashAccumData, groupId);

        // Assert
        Assert.Equal(errCode, result.ErrCode);
        Assert.Equal(errMsg, result.ErrMsg);
        _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains(errMsg))), Times.Once);
    }

    [Fact]
    public void GetBatchSize_WithBatchSizeParameter_ReturnsCorrectValue()
    {
        // Arrange
        var scheduleJob = new ScheduleJob
        {
            Parameters = new[]
            {
                new ScheduleJobParameter { Key = "BatchSize", Value = "100" }
            }
        };

        // Act
        var result = AccumulatedCreditService.GetBatchSize(scheduleJob);

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void GetBatchSize_WithoutBatchSizeParameter_ReturnsDefaultValue()
    {
        // Arrange
        var scheduleJob = new ScheduleJob
        {
            Parameters = Array.Empty<ScheduleJobParameter>()
        };

        // Act
        var result = AccumulatedCreditService.GetBatchSize(scheduleJob);

        // Assert
        Assert.Equal(500, result);
    }
}
