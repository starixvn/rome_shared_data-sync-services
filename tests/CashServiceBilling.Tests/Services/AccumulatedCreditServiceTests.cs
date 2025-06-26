using System.Text.Json;
using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using CashServiceBilling.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CashServiceBilling.Tests.Services;

public class AccumulatedCreditServiceTests
{
    private readonly Mock<IDynamicRepository> mockRepository;
    private readonly Mock<Fanex.Logging.ILogger> mockLogger;
    private readonly Mock<IConfiguration> mockConfiguration;
    private readonly Mock<ITransactionLogService> mockTransactionLogService;
    private readonly Mock<IConfigurationSection> mockConfigurationSection;
    private readonly AccumulatedCreditService service;

    public AccumulatedCreditServiceTests()
    {
        mockRepository = new Mock<IDynamicRepository>();
        mockLogger = new Mock<Fanex.Logging.ILogger>();
        mockConfiguration = new Mock<IConfiguration>();
        mockTransactionLogService = new Mock<ITransactionLogService>();
        mockConfigurationSection = new Mock<IConfigurationSection>();

        mockConfigurationSection.Setup(x => x.Value).Returns("TestCategory");
        mockConfiguration.Setup(x => x.GetSection("JobsSettings:NotifyCategory")).Returns(mockConfigurationSection.Object);
        // Replace GetValue with GetSection().Value since GetValue is an extension method
        mockConfiguration.Setup(x => x.GetSection("JobsSettings:NotifyCategory").Value).Returns("TestCategory");

        service = new AccumulatedCreditService(mockRepository.Object, mockLogger.Object, mockConfiguration.Object, mockTransactionLogService.Object);
    }

    [Fact]
    public async Task GetMemberYesterdayWinLoss_ShouldExecuteCriteria_AndReturnCustIdList()
    {
        // Arrange
        const string expectedCustIdList = "[{\"CustId\":123,\"Amount\":100.0}]";
        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                var criteria = c as GetCashMemberListCriteria;
                criteria.CustIdList = expectedCustIdList;
            })
            .Returns(Task.FromResult(0));

        // Act
        var result = await service.GetMemberYesterdayWinLoss(false);

        // Assert
        Assert.Equal(expectedCustIdList, result);
        mockRepository.Verify(r => r.ExecuteAsync(It.Is<GetCashMemberListCriteria>(c => c.IsTest == false)), Times.Once);
    }

    [Fact]
    public async Task AddManualDepositTrans_ShouldFetchLogs_AndReturnTupleWithResults()
    {
        // Arrange
        const string custIdList = "[{\"CustId\":123,\"Amount\":100.0}]";
        const string expectedCashAccumCreditTbl = "[{\"CustId\":123,\"Amount\":100.0,\"CustDBGroupId\":1}]";
        var expectedLogs = new List<LogEntry> { new LogEntry { TrackingValueMask = "Test log" } };

        mockRepository.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<ICriteria>()))
            .Callback<ICriteria>(c =>
            {
                var criteria = c as AddManualDepositTransCriteria;
                criteria.CashAccumCreditTbl = expectedCashAccumCreditTbl;
            })
            .ReturnsAsync(expectedLogs);

        // Act
        var result = await service.AddManualDepositTrans(custIdList);

        // Assert
        Assert.Equal(expectedCashAccumCreditTbl, result.Item1);
        Assert.Same(expectedLogs, result.Item2);
        mockRepository.Verify(r => r.FetchAsync<LogEntry>(It.Is<AddManualDepositTransCriteria>(c => c.CustIdList == custIdList)), Times.Once);
    }

    [Fact]
    public async Task SyncCashAccumCredit_WhenSuccess_ShouldReturnSuccessResult()
    {
        // Arrange
        const string cashAccumCreditTbl = "[{\"CustId\":123,\"Amount\":100.0,\"CustDBGroupId\":1}]";
        const short custDBGroupId = 1;

        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                var criteria = c as SyncCashAccumCreditCriteria;
                criteria.ErrCode = 0;
                criteria.ErrMsg = string.Empty;
            })
            .Returns(Task.FromResult(0));

        // Act
        var result = await service.SyncCashAccumCredit(cashAccumCreditTbl, custDBGroupId);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.Equal(string.Empty, result.ErrMsg);
        mockRepository.Verify(r => r.ExecuteAsync(It.Is<SyncCashAccumCreditCriteria>(
            c => c.CashAccumCreditTbl == cashAccumCreditTbl && c.CustDBGroupId == custDBGroupId)), Times.Once);
        mockLogger.Verify(l => l.ErrorAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SyncCashAccumCredit_WhenError_ShouldLogError()
    {
        // Arrange
        const string cashAccumCreditTbl = "[{\"CustId\":123,\"Amount\":100.0,\"CustDBGroupId\":1}]";
        const short custDBGroupId = 1;

        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                var criteria = c as SyncCashAccumCreditCriteria;
                criteria.ErrCode = 1;
                criteria.ErrMsg = "Error occurred";
            })
            .Returns(Task.FromResult(0));

        // Act
        var result = await service.SyncCashAccumCredit(cashAccumCreditTbl, custDBGroupId);

        // Assert
        Assert.Equal(1, result.ErrCode);
        Assert.Equal("Error occurred", result.ErrMsg);
        mockLogger.Verify(l => l.ErrorAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AccumulateCreditToAgent_WhenNoWinLoss_ShouldOnlyLogStartAndComplete()
    {
        // Arrange
        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                var criteria = c as GetCashMemberListCriteria;
                criteria.CustIdList = string.Empty;
            })
            .Returns(Task.FromResult(0));

        // Act
        await service.AccumulateCreditToAgent(false, 100);

        // Assert
        mockLogger.Verify(l => l.ErrorAsync(It.IsAny<string>(), "TestCategory"), Times.Exactly(3));
    }

    [Fact]
    public async Task AccumulateCreditToAgent_WhenHasWinLoss_ShouldProcessData()
    {
        // Arrange
        var memberWinLoss = new[]
        {
                new MemberYesterdayWL { CustId = 123, YesterdayWL = 100 },
                new MemberYesterdayWL { CustId = 456, YesterdayWL = 200 }
            };

        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<INonQueryCommand>()))
            .Callback<INonQueryCommand>(c =>
            {
                if (c is GetCashMemberListCriteria)
                {
                    var criteria = c as GetCashMemberListCriteria;
                    criteria.CustIdList = JsonSerializer.Serialize(memberWinLoss);
                }
                else if (c is SyncCashAccumCreditCriteria) // Setup for SyncCashAccumCredit
                {
                    var criteria = c as SyncCashAccumCreditCriteria;
                    criteria.ErrCode = 0;
                    criteria.ErrMsg = string.Empty;
                }
            })
            .Returns(Task.FromResult(0));

        // Setup for AddManualDepositTrans
        mockRepository.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<ICriteria>()))
            .Callback<ICriteria>(c =>
            {
                var criteria = c as AddManualDepositTransCriteria;
                criteria.CashAccumCreditTbl = JsonSerializer.Serialize(
                new[]
                {
                    new CashAccumCredit { CustId = 123, AccumCreditReturn = 100, CustDBGroupId = 1 },
                    new CashAccumCredit { CustId = 456, AccumCreditReturn = 200, CustDBGroupId = 1 }
                });
            })
            .ReturnsAsync(new List<LogEntry> { new LogEntry { TrackingValue = "Test log" } });

        // Act
        await service.AccumulateCreditToAgent(false, 10);

        // Assert
        mockTransactionLogService.Verify(t => t.WriteLogs(It.IsAny<IEnumerable<LogEntry>>()), Times.Once);
        mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<SyncCashAccumCreditCriteria>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMemberYesterdayWinLoss_ShouldCallAddManualDepositTransAndSyncCashAccumCredit()
    {
        // Arrange
        var memberWinLoss = new[]
        {
            new MemberYesterdayWL { CustId = 123, YesterdayWL = 100 }
        };

        var cashAccumCredit = new[]
        {
            new CashAccumCredit { CustId = 123, AccumCreditReturn = 100, CustDBGroupId = 1 }
        };

        mockRepository.Setup(r => r.FetchAsync<LogEntry>(It.IsAny<ICriteria>()))
            .Callback<ICriteria>(c =>
            {
                var criteria = c as AddManualDepositTransCriteria;
                criteria.CashAccumCreditTbl = JsonSerializer.Serialize(cashAccumCredit);
            })
            .ReturnsAsync(new List<LogEntry> { new LogEntry { TrackingValue = "Test log" } });

        mockRepository.Setup(r => r.ExecuteAsync(It.IsAny<SyncCashAccumCreditCriteria>()))
            .Returns(Task.FromResult(0));

        // Act
        await service.ProcessMemberYesterdayWinLoss(memberWinLoss);

        // Assert
        mockRepository.Verify(r => r.FetchAsync<LogEntry>(It.IsAny<AddManualDepositTransCriteria>()), Times.Once);
        mockRepository.Verify(r => r.ExecuteAsync(It.IsAny<SyncCashAccumCreditCriteria>()), Times.Once);
        mockTransactionLogService.Verify(t => t.WriteLogs(It.IsAny<IEnumerable<LogEntry>>()), Times.Once);
    }
}