using BalanceRecord.Jobs;
using BalanceRecord.Services;
using NSubstitute;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;
using Xunit;

namespace BalanceRecord.Tests.Jobs;

public class BalanceRecordJobTests
{
    private readonly ScheduleJob scheduleJob;
    private readonly ILoggerService loggerService;
    private readonly IMaintenanceService maintenanceService;
    private readonly IBalanceRecordService balanceRecordService;
    private readonly BalanceRecordJob balanceRecordJob;

    public BalanceRecordJobTests()
    {
        scheduleJob = new ScheduleJob
        {
            Name = "SiteDB01",
            Parameters = new[]
            {
                new ScheduleJobParameter { Key = "SiteDBGroupId", Value = "1" },
                new ScheduleJobParameter { Key = "BatchSize", Value = "2000" }
            },
            DelayExecutionTimeInSeconds = 1,
            IsLogAfterExecuteJob = true,
            IsActive = true
        };
        loggerService = Substitute.For<ILoggerService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        balanceRecordService = Substitute.For<IBalanceRecordService>();
        balanceRecordJob = new BalanceRecordJob(
            scheduleJob,
            loggerService,
            maintenanceService,
            new UMClientConfiguration { ResultCacheDuration = 60 },
            balanceRecordService);
    }

    [Fact]
    public async Task ExecuteJob_IsStopJobIsTrue_CallsLogInfo()
    {
        // Arrange
        balanceRecordService
            .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(true));

        // Act
        await balanceRecordJob.ExecuteJob();

        // Assert
        await loggerService.Received().Info(Arg.Is<string>(
            arg => "SiteDB01 is set stopped from DB." == arg
        ));
    }

    [Fact]
    public async Task ExecuteJob_GetLastRowInsertedIsNull_CallsLogInfo()
    {
        // Arrange
        balanceRecordService
            .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(false));
        balanceRecordService
            .GetLastRowInserted(Arg.Any<short>(), Arg.Any<bool>())
            .Returns(Task.FromResult(default(long?)));

        // Act
        await balanceRecordJob.ExecuteJob();

        // Assert
        await loggerService.Received().Info(Arg.Is<string>(
            arg => "SiteDB01: LastRowInserted is NULL." == arg
        ));
    }

    [Fact]
    public async Task ExecuteJob_GetDataFromSiteDBIsNullOrEmpty_ReturnsFalse()
    {
        // Arrange
        balanceRecordService
            .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(false));
        balanceRecordService
            .GetLastRowInserted(Arg.Any<short>(), Arg.Any<bool>())
            .Returns(Task.FromResult<long?>(1));
        balanceRecordService
            .GetDataFromSiteDB(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(Task.FromResult(Array.Empty<byte>()));

        // Act
        var actual = await balanceRecordJob.ExecuteJob();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task ExecuteJob_InsertDataIntoSiteDBError_CallsLogError()
    {
        // Arrange
        balanceRecordService
            .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(false));
        balanceRecordService
            .GetLastRowInserted(Arg.Any<short>(), Arg.Any<bool>())
            .Returns(Task.FromResult<long?>(1));
        balanceRecordService
            .GetDataFromSiteDB(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(Task.FromResult(new byte[] { 1, 2, 3, 4, 5, 6 }));
        balanceRecordService
            .InsertDataIntoSiteDB(Arg.Any<short>(), Arg.Any<byte[]>(), Arg.Any<bool>())
            .Returns(Task.FromResult("Invalid object odb"));

        // Act
        await balanceRecordJob.ExecuteJob();

        // Assert
        await loggerService.Received().Error(
            Arg.Any<Exception>(),
            Arg.Any<ScheduleJob>(),
            "InsertDataIntoSiteDB",
            "Invalid object odb");
    }

    [Fact]
    public async Task ExecuteJob_InsertDataIntoSiteDBSuccessfully_ReturnsTrue()
    {
        // Arrange
        balanceRecordService
            .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(false));
        balanceRecordService
            .GetLastRowInserted(Arg.Any<short>(), Arg.Any<bool>())
            .Returns(Task.FromResult<long?>(1));
        balanceRecordService
            .GetDataFromSiteDB(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(Task.FromResult(new byte[] { 1, 2, 3, 4, 5, 6 }));
        balanceRecordService
            .InsertDataIntoSiteDB(Arg.Any<short>(), Arg.Any<byte[]>(), Arg.Any<bool>())
            .Returns(Task.FromResult(string.Empty));

        // Act
        var actual = await balanceRecordJob.ExecuteJob();

        // Assert
        Assert.True(actual);
    }
}
