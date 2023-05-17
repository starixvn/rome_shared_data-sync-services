using BalanceRecord.Criteria;
using BalanceRecord.Services;
using Fanex.Data.Repository;
using NSubstitute;
using Xunit;

namespace BalanceRecord.Tests.Services;

public class BalanceRecordServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly IBalanceRecordService balanceRecordService;

    public BalanceRecordServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        balanceRecordService = new BalanceRecordService(dynamicRepository);
    }

    [Fact]
    public async Task IsStopJob_Always_CallsExecuteAsync()
    {
        // Arrange
        var jobName = "SiteDB01";
        var isTest = false;

        // Act
        await balanceRecordService.IsStopJob(jobName, isTest);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<GetJobStatusCommand>(
            criteria => "BT_SiteDB01_ServiceStatus" == criteria.VName
        ));
    }

    [Fact]
    public async Task GetLastRowInserted_Always_CallsExecuteAsync()
    {
        // Arrange
        short siteDBGroupId = 1;
        var isTest = false;

        // Act
        await balanceRecordService.GetLastRowInserted(siteDBGroupId, isTest);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<GetLastRowInsertedCommand>(
            criteria => siteDBGroupId == criteria.SiteDBGroupId
            && isTest == criteria.IsTest
        ));
    }

    [Fact]
    public async Task GetDataFromSiteDB_Always_CallsExecuteAsync()
    {
        // Arrange
        var siteDBName = "SiteDB01";
        long lastRowInserted = 1;
        var batchSize = 2000;
        var isTest = false;

        // Act
        await balanceRecordService.GetDataFromSiteDB(siteDBName, lastRowInserted, batchSize, isTest);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<GetBalanceRecordFromSiteDBCommand>(
            criteria => siteDBName == criteria.SiteDBName
            && lastRowInserted == criteria.LastRowInserted
            && batchSize == criteria.Batch
            && isTest == criteria.IsTest
        ));
    }

    [Fact]
    public async Task InsertDataIntoSiteDB_Always_CallsExecuteAsync()
    {
        // Arrange
        short siteDBGroupId = 1;
        var transInfo = new byte[] { 1 };
        var isTest = false;

        // Act
        await balanceRecordService.InsertDataIntoSiteDB(siteDBGroupId, transInfo, isTest);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<InsertBalanceRecordFromSiteDBCommand>(
            criteria => siteDBGroupId == criteria.SiteDBGroupId
            && 1 == criteria.TransInfo[0]
            && isTest == criteria.IsTest
        ));
    }
}
