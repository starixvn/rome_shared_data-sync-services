using AutoTransfer.Constants;
using AutoTransfer.Jobs;
using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;
using static Dapper.SqlMapper;


namespace AutoTransfer.Tests.Jobs;

public class AutoTransferJobTests
{
    private readonly ILoggerService loggerService;
    private readonly IMaintenanceService maintenanceService;
    private readonly ITransferService transferService;
    private readonly AutoTransferJob autoTransferJob;
    private readonly IOptions<JobsSettingsModel> settings;

    public AutoTransferJobTests()
    {

        settings = Substitute.For<IOptions<JobsSettingsModel>>();

        loggerService = Substitute.For<ILoggerService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        transferService = Substitute.For<ITransferService>();
        autoTransferJob = new AutoTransferJob(
            GetListScheduleJob(),
            loggerService,
            maintenanceService,
            new UMClientConfiguration { ResultCacheDuration = 60 },
            transferService, settings);


    }

    [Fact]
    public async Task ExecuteInternal_IsCompleteIsTrue_CallsLogInfo()
    {
        ScheduleJob scheduleJob = new ScheduleJob()
        {
            Name = "AutoTransferJob",
            DelayExecutionTimeInSeconds = 1,
            IsActive = true,
            DelayExecutionAtExactlyTime = new TimeOnly(5, 12, 57),
        };
        settings.Value.Returns(new JobsSettingsModel()
        {
            NumberOfThreads = 1,
            PairOfBatch = 200,

        });
        CancellationTokenSource cts = new();
        CancellationToken cancellationToken = cts.Token;
        // Arrange
        transferService
            .Start(settings.Value.PairOfBatch, RunningModeConstant.Normal)
            .Returns(Task.FromResult(new AutoTransferStartModel()
            {
                TotalBatch = 1,
            }));

        // Act
        await autoTransferJob.ExecuteInternal(cancellationToken);

        // Assert
        loggerService.Received().InfoConsole(
           Arg.Is<string>(arg => "{JobInfo} run completed." == arg),
           Arg.Is<object[]>(arg => scheduleJob.JobInfo == arg[0].ToString())
       );


    }

    //[Fact]
    //public async Task ExecuteJob_GetLastRowInsertedIsNull_CallsLogInfo()
    //{
    //    // Arrange
    //    transferService
    //        .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(false));
    //    transferService
    //        .GetLastRowInserted(Arg.Any<short>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(default(long?)));

    //    // Act
    //    await autoTransferJob.ExecuteJob();

    //    // Assert
    //    await loggerService.Received().Info(Arg.Is<string>(
    //        arg => "[SiteDB01]: LastRowInserted is NULL." == arg
    //    ));
    //}

    //[Fact]
    //public async Task ExecuteJob_GetDataFromSiteDBIsNullOrEmpty_ReturnsFalse()
    //{
    //    // Arrange
    //    transferService
    //        .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(false));
    //    transferService
    //        .GetLastRowInserted(Arg.Any<short>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult<long?>(1));
    //    transferService
    //        .GetDataFromSiteDB(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(Array.Empty<byte>()));

    //    // Act
    //    var actual = await autoTransferJob.ExecuteJob();

    //    // Assert
    //    Assert.False(actual);
    //}

    //[Fact]
    //public async Task ExecuteJob_InsertDataIntoSiteDBError_CallsLogError()
    //{
    //    // Arrange
    //    transferService
    //        .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(false));
    //    transferService
    //        .GetLastRowInserted(Arg.Any<short>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult<long?>(1));
    //    transferService
    //        .GetDataFromSiteDB(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(new byte[] { 1, 2, 3, 4, 5, 6 }));
    //    transferService
    //        .InsertDataIntoSiteDB(Arg.Any<short>(), Arg.Any<byte[]>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult("Invalid object odb"));

    //    // Act
    //    await autoTransferJob.ExecuteJob();

    //    // Assert
    //    await loggerService.Received().Error(
    //        Arg.Any<Exception>(),
    //        Arg.Any<ScheduleJob>(),
    //        "InsertDataIntoSiteDB",
    //        "Invalid object odb");
    //}

    //[Fact]
    //public async Task ExecuteJob_InsertDataIntoSiteDBSuccessfully_ReturnsTrue()
    //{
    //    // Arrange
    //    transferService
    //        .IsStopJob(Arg.Any<string>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(false));
    //    transferService
    //        .GetLastRowInserted(Arg.Any<short>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult<long?>(1));
    //    transferService
    //        .GetDataFromSiteDB(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(new byte[] { 1, 2, 3, 4, 5, 6 }));
    //    transferService
    //        .InsertDataIntoSiteDB(Arg.Any<short>(), Arg.Any<byte[]>(), Arg.Any<bool>())
    //        .Returns(Task.FromResult(string.Empty));

    //    // Act
    //    var actual = await autoTransferJob.ExecuteJob();

    //    // Assert
    //    Assert.True(actual);
    //}

    private ScheduleJob[] GetListScheduleJob()
    {
        return new ScheduleJob[4]{
            new ScheduleJob()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(5, 12, 57),
            },
            new ScheduleJob()
            {
                Name = "AutoTransferRetryJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(5, 12, 57),
            },
            new ScheduleJob()
            {
                Name = "ResumeTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(5, 12, 57),
            },
            new ScheduleJob()
            {
                Name = "ResumeTransferRetryJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(5, 12, 57),
            },
        };
    }
}
