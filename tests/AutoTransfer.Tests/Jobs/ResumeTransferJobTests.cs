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
    public class ResumeTransferJobTests
    {
        private readonly ILoggerService loggerService;
        private readonly IMaintenanceService maintenanceService;
        private readonly ITransferService transferService;
        private readonly ResumeTransferJob autoTransferJob;
        private readonly IOptions<JobsSettingsModel> settings;

        public ResumeTransferJobTests()
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
            autoTransferJob = new ResumeTransferJob(
                ScheduleJobData.GetData(),
                loggerService,
                maintenanceService,
                new UMClientConfiguration { ResultCacheDuration = 60 },
                transferService, settings);
        }

        [Fact]
        public async Task ExecuteInternal_RunTimeAndHaveNoBatch_LogCompleted()
        {
            // Arrange
            ScheduleJob scheduleJob = new()
            {
                Name = "ResumeTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyDateTime = DateTime.Now
            };

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime()
            {
                IsUnderMaintenanceTime = false,
            }));

            transferService
                .Start(settings.Value.PairOfBatch, RunningModeConstant.Resume)
                .Returns(Task.FromResult(new AutoTransferStartModel()
                {
                    TotalBatch = 0,
                }));

            // Act
            await autoTransferJob.ExecuteInternal(cancellationToken);

            // Assert
            loggerService.Received().InfoConsole(
               Arg.Is<string>(arg => "{JobInfo} run completed." == arg),
               Arg.Is<object[]>(arg => scheduleJob.JobInfo == arg[0].ToString())
           );
        }

        [Fact]
        public async Task ExecuteInternal_RunTimeAndHaveBatch_LogCompleted()
        {
            // Arrange
            ScheduleJob scheduleJob = new()
            {
                Name = "ResumeTransferJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyDateTime = DateTime.Now
            };

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime()
            {
                IsUnderMaintenanceTime = false,
            }));

            transferService
                .Start(settings.Value.PairOfBatch, RunningModeConstant.Resume)
                .Returns(Task.FromResult(new AutoTransferStartModel()
                {
                    TotalBatch = 1,
                }));

            var ci = 0;
            transferService
                .SelectBatches(settings.Value.NumberOfThreads, RunningModeConstant.Resume)
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
            loggerService.Received().InfoConsole(
               Arg.Is<string>(arg => "{JobInfo} run completed." == arg),
               Arg.Is<object[]>(arg => scheduleJob.JobInfo == arg[0].ToString())
           );
        }
    }
}