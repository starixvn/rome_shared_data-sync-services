using AutoTransfer.Constants;
using AutoTransfer.Jobs;
using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;


namespace AutoTransfer.Tests.Jobs
{
    public class BaseAutoTransferJobTests
    {
        private readonly ILoggerService loggerService;
        private readonly IMaintenanceService maintenanceService;
        private readonly ITransferService transferService;
        private readonly BaseAutoTransferJob autoTransferJob;

        private readonly JobsSettingsModel js = new()
        {
            NumberOfThreads = 1,
            PairOfBatch = 200
        };

        private readonly ScheduleJob sc = new()
        {
            Name = "AutoTransferJob",
            DelayExecutionTimeInSeconds = 1,
            IsActive = true,
        };

        private readonly IOptions<JobsSettingsModel> settings;

        public BaseAutoTransferJobTests()
        {
            settings = Substitute.For<IOptions<JobsSettingsModel>>();
            _ = settings.Value.Returns(js);
            loggerService = Substitute.For<ILoggerService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            transferService = Substitute.For<ITransferService>();
            autoTransferJob = new BaseAutoTransferJob(
                sc,
                loggerService,
                maintenanceService,
                new UMClientConfiguration { ResultCacheDuration = 60 },
                transferService, settings);
        }

        [Fact]
        public async Task ExecuteInternal_Always_ThrowNotImplementedException()
        {
            // Arrange
            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(async () => await autoTransferJob.ExecuteInternal(cancellationToken));
        }

        [Fact]
        public async Task WaitForSystemUM_IsNotUM_DoNothing()
        {
            // Arrange
            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime()
            {
                IsUnderMaintenanceTime = false,
            }));

            // Act
            await autoTransferJob.WaitForSystemUM(cancellationToken);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task WaitForSystemUM_IsUM_CompleteAfterUM()
        {
            // Arrange
            ScheduleJob scheduleJob = new()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
            };

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime()
            {
                IsUnderMaintenanceTime = true,
            }));

            var ci = 0;
            maintenanceService
                .GetUnderMaintenanceTimeAsync()
                .Returns(c =>
                {
                    if (ci == 1)
                    {
                        ci++;
                        return new UnderMaintenanceTime { IsUnderMaintenanceTime = false };
                    }

                    var ct = DateTime.Now;
                    ci++;
                    return new UnderMaintenanceTime
                    {
                        IsUnderMaintenanceTime = true,
                        From = "From"
                    };
                });

            // Act
            await autoTransferJob.WaitForSystemUM(cancellationToken);

            // Assert
            Assert.True(ci == 2);
        }

        [Fact]
        public async Task WaitForBalanceUpProcess_Always_CheckBalanceUpProcess()
        {
            // Arrange
            ScheduleJob scheduleJob = new()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
            };

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            var ci = 0;
            transferService
                .CheckBalanceUpProcess()
                .Returns(x =>
                {
                    if (ci == 1)
                    {
                        ci++;
                        return (DateTime?)null;
                    }

                    var ct = DateTime.Now;
                    ci++;
                    return new DateTime(ct.Year, ct.Month, ct.Day, ct.Hour, ct.Minute, ct.Second + 1);
                });

            // Act
            await autoTransferJob.WaitForBalanceUpProcess(cancellationToken);

            // Assert
            loggerService.Received().InfoConsole(Arg.Any<string>());
        }

        [Fact]
        public void IsRunTime_DifferentHour_ReturnsFalse()
        {
            // Arrange
            var runTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute).AddHours(1);

            // Act
            var actualValue = autoTransferJob.IsRunTime(runTime, DateTime.Now);

            // Assert
            Assert.False(actualValue);
        }

        [Fact]
        public void IsRunTime_DifferentMinute_ReturnsFalse()
        {
            // Arrange
            var runTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute).AddMinutes(5);

            // Act
            var actualValue = autoTransferJob.IsRunTime(runTime, DateTime.Now);

            // Assert
            Assert.False(actualValue);
        }

        [Fact]
        public void IsRunTime_RunTime_ReturnsTrue()
        {
            // Arrange
            var runTime = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute);

            // Act
            var actualValue = autoTransferJob.IsRunTime(runTime, DateTime.Now);

            // Assert
            Assert.True(actualValue);
        }
    }
}