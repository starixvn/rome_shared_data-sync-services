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
    public class AutoTransferRetryJobTests
    {
        private readonly int currentHour = DateTime.Now.Hour;
        private readonly int currentMin = DateTime.Now.Minute;

        private readonly ILoggerService loggerService;
        private readonly IMaintenanceService maintenanceService;
        private readonly ITransferService transferService;
        private readonly AutoTransferRetryJob autoTransferRetryJob;
        private readonly IOptions<JobsSettingsModel> settings;

        public AutoTransferRetryJobTests()
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
            autoTransferRetryJob = new AutoTransferRetryJob(
                ScheduleJobData.GetData(),
                loggerService,
                maintenanceService,
                new UMClientConfiguration { ResultCacheDuration = 60 },
                transferService, settings);
        }

        [Fact]
        public async Task ExecuteInternalRunTimeLogCompleted()
        {
            // Arrange
            ScheduleJob scheduleJob = new()
            {
                Name = "AutoTransferRetryJob",
                DelayExecutionTimeInSeconds = 1,
                IsActive = true,
                DelayExecutionAtExactlyTime = new TimeOnly(currentHour, currentMin, 0),
            };

            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime()
            {
                IsUnderMaintenanceTime = false,
            }));

            int ci = 0;
            transferService
                .SelectRetryBatches(settings.Value.NumberOfThreads)
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
                        return new RetryBatchesModel { Batches = new List<BatchModel> { new BatchModel() } };

                    }

                    ci++;
                    return new RetryBatchesModel { IsCompleted = true };
                });


            int ii = 0;
            transferService.When(x => x.ImportLogs(Arg.Any<int>()))
                .Do(x =>
                {
                    ii++;
                    if (ii == 2)
                    {
                        throw new Exception();
                    }
                });

            // Act
            await autoTransferRetryJob.ExecuteInternal(cancellationToken);

            // Assert
            loggerService.Received().InfoConsole(
               Arg.Is<string>(arg => "{JobInfo} run completed." == arg),
               Arg.Is<object[]>(arg => scheduleJob.JobInfo == arg[0].ToString())
           );
        }
    }
}