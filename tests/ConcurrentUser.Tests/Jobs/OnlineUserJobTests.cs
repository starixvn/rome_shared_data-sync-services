using ConcurrentUser.Jobs;
using ConcurrentUser.Models;
using ConcurrentUser.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;
using NSubstitute;
using Xunit;
using Shared.Models;

namespace ConcurrentUser.Tests.Jobs;

public class OnlineUserJobTests
{
    private readonly ILoggerService loggerService;
    private readonly IMaintenanceService maintenanceService;
    private readonly IOnlineUserService onlineUserService;
    private readonly ScheduleJob scheduleJob;
    private readonly UMClientConfiguration umClientConfiguration;

    public OnlineUserJobTests()
    {
        loggerService = Substitute.For<ILoggerService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        onlineUserService = Substitute.For<IOnlineUserService>();
        umClientConfiguration = new UMClientConfiguration { ResultCacheDuration = 60 };
        scheduleJob = new ScheduleJob
        {
            Name = "OnlineUser",
            IsActive = true,
            DelayExecutionTimeInSeconds = 1,
            Parameters = Array.Empty<ScheduleJobParameter>()
        };
    }

    [Fact]
    public async Task ExecuteInternal_WhenNotUnderMaintenance_CallsGetOnlineUserAndInsert()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>
        {
            new ConcurrentUserModel
            {
                CreatedDate = DateTime.Now,
                RealUserCount = 10,
                RoleId = 2,
                SiteId = 100,
                TestingUserCount = 20
            }
        };

        maintenanceService.GetUnderMaintenanceTimeAsync()
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });
        onlineUserService.GetOnlineUser().Returns(users);

        var job = new OnlineUserJob(
            scheduleJob,
            loggerService,
            maintenanceService,
            umClientConfiguration,
            onlineUserService);

        using var cts = new CancellationTokenSource();

        // Act
        await job.ExecuteInternal(cts.Token);

        // Assert
        await onlineUserService.Received(1).GetOnlineUser();
        await onlineUserService.Received(1).InsertOnlineUser(users);
    }

    [Fact]
    public async Task ExecuteInternal_WhenUnderMaintenance_DoesNotCallService()
    {
        // Arrange
        maintenanceService.GetUnderMaintenanceTimeAsync()
            .Returns(new UnderMaintenanceTime
            {
                IsUnderMaintenanceTime = true,
                From = DateTime.Now.ToString("o"),
                To = DateTime.Now.AddHours(1).ToString("o")
            });

        var job = new OnlineUserJob(
            scheduleJob,
            loggerService,
            maintenanceService,
            umClientConfiguration,
            onlineUserService);

        using var cts = new CancellationTokenSource();

        // Act
        await job.ExecuteInternal(cts.Token);

        // Assert
        await onlineUserService.DidNotReceive().GetOnlineUser();
        await onlineUserService.DidNotReceive().InsertOnlineUser(Arg.Any<IEnumerable<ConcurrentUserModel>>());
    }

    [Fact]
    public async Task ExecuteInternal_WhenInsertFails_LogsError()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>
        {
            new ConcurrentUserModel
            {
                CreatedDate = DateTime.Now,
                RealUserCount = 10,
                RoleId = 2,
                SiteId = 100,
                TestingUserCount = 20
            }
        };
        var expectedException = new Exception("Insert failed");

        maintenanceService.GetUnderMaintenanceTimeAsync()
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });
        onlineUserService.GetOnlineUser().Returns(users);
        onlineUserService.InsertOnlineUser(users).Returns(_ => Task.FromException<Result>(expectedException));

        var job = new OnlineUserJob(
            scheduleJob,
            loggerService,
            maintenanceService,
            umClientConfiguration,
            onlineUserService);

        using var cts = new CancellationTokenSource();

        // Act
        await job.ExecuteInternal(cts.Token);

        // Assert
        await loggerService.Received(1).Error(
            Arg.Is<Exception>(ex => ex.Message == "Insert failed"),
            scheduleJob,
            "InsertOnlineUser");
    }

    [Fact]
    public async Task ExecuteInternal_WhenGetUsersFails_LogsException()
    {
        // Arrange
        var expectedException = new Exception("Get users failed");

        maintenanceService.GetUnderMaintenanceTimeAsync()
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });
        onlineUserService.GetOnlineUser().Returns(Task.FromException<IEnumerable<ConcurrentUserModel>>(expectedException));

        var job = new OnlineUserJob(
            scheduleJob,
            loggerService,
            maintenanceService,
            umClientConfiguration,
            onlineUserService);

        using var cts = new CancellationTokenSource();

        // Act
        await job.ExecuteInternal(cts.Token);

        // Assert
        await loggerService.Received(1).Error(
            Arg.Is<Exception>(ex => ex.Message == "Get users failed"),
            scheduleJob,
            "ExecuteInternal");
    }

    [Fact]
    public async Task ExecuteInternal_Always_DelaysExecution()
    {
        // Arrange
        maintenanceService.GetUnderMaintenanceTimeAsync()
            .Returns(new UnderMaintenanceTime { IsUnderMaintenanceTime = false });
        onlineUserService.GetOnlineUser().Returns(new List<ConcurrentUserModel>());

        var job = new OnlineUserJob(
            scheduleJob,
            loggerService,
            maintenanceService,
            umClientConfiguration,
            onlineUserService);

        using var cts = new CancellationTokenSource();
        var startTime = DateTime.Now;

        // Act
        await job.ExecuteInternal(cts.Token);
        var endTime = DateTime.Now;

        // Assert
        var elapsed = (endTime - startTime).TotalSeconds;
        Assert.True(elapsed >= scheduleJob.DelayExecutionTimeInSeconds);
    }
}
