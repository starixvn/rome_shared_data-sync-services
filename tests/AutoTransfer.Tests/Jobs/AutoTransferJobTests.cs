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
            _ = settings.Value.Returns(new JobsSettingsModel()
            {
                NumberOfThreads = 1,
                PairOfBatch = 200
            });
            loggerService = Substitute.For<ILoggerService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            transferService = Substitute.For<ITransferService>();
            autoTransferJob = new AutoTransferJob(
                ScheduleJobData.GetData(),
                loggerService,
                maintenanceService,
                new UMClientConfiguration { ResultCacheDuration = 60 },
                transferService, settings);
        }

        [Fact]
        public async Task ExecuteInternal_NotRunTime_NotLogCompleted()
        {
            // Arrange
            ScheduleJob scheduleJob = new()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
            };

            var time = DateTime.Now.AddHours(1);
            transferService.GetJobConfigModel(Arg.Any<string>()).Returns(new JobConfigModel
            {
                DefaultTime = $"{time.Hour}:{time.Minute}"
            });

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            // Act
            await autoTransferJob.ExecuteInternal(cancellationToken);

            // Assert
            _ = loggerService.DidNotReceive().Error(
                Arg.Is<Exception>(e => e.Message == "[AutoTransferJob] completed transferring."),
                Arg.Any<ScheduleJob>(),
                Arg.Any<string>());
        }

        [Fact]
        public async Task ExecuteInternal_RunTimeAndHaveNoBatch_LogCompleted()
        {
            // Arrange
            ScheduleJob scheduleJob = new()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
            };

            transferService.GetJobConfigModel(Arg.Any<string>()).Returns(new JobConfigModel
            {
                DefaultTime = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}",
                IsActive = true
            });

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime()
            {
                IsUnderMaintenanceTime = false,
            }));

            transferService
                .Start(settings.Value.PairOfBatch, RunningModeConstant.Resume, Arg.Any<string>())
                .Returns(Task.FromResult(new AutoTransferStartModel()
                {
                    TotalBatch = 0,
                }));

            // Act
            await autoTransferJob.ExecuteInternal(cancellationToken);

            // Assert
            _ = loggerService.Received().Error(
                Arg.Is<Exception>(e => e.Message == "[AutoTransferJob] completed transferring."),
                Arg.Any<ScheduleJob>(),
                Arg.Any<string>());
        }

        [Fact]
        public async Task ExecuteInternal_RunTimeAndHaveBatch_LogCompleted()
        {
            // Arrange
            ScheduleJob scheduleJob = new()
            {
                Name = "AutoTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
            };

            transferService.GetJobConfigModel(Arg.Any<string>()).Returns(new JobConfigModel
            {
                DefaultTime = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}",
                IsActive = true
            });

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime()
            {
                IsUnderMaintenanceTime = false,
            }));

            transferService
                .Start(settings.Value.PairOfBatch, RunningModeConstant.Resume, Arg.Any<string>())
                .Returns(Task.FromResult(new AutoTransferStartModel()
                {
                    TotalBatch = 1,
                }));

            var ci = 0;
            transferService
                .SelectBatches(settings.Value.NumberOfThreads, RunningModeConstant.Normal, Arg.Any<string>())
                .Returns(c =>
                {
                    if (ci == 1)
                    {
                        ci++;
                        throw new Exception();
                    }

                    if (ci == 0)
                    {
                        ci++;
                        return new List<BatchModel> { new BatchModel() };
                    }

                    ci++;
                    return new List<BatchModel> { };
                });

            // Act
            await autoTransferJob.ExecuteInternal(cancellationToken);

            // Assert
            _ = loggerService.Received().Error(
                Arg.Is<Exception>(e => e.Message == "[AutoTransferJob] completed transferring."),
                Arg.Any<ScheduleJob>(),
                Arg.Any<string>());
        }
    }
}