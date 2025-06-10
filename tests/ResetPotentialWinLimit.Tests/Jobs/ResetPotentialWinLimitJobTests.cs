using Microsoft.Extensions.Options;
using Moq;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using ResetPotentialWinLimit.Extensions;
using ResetPotentialWinLimit.Jobs;
using ResetPotentialWinLimit.Models;
using ResetPotentialWinLimit.Services;
using Shared.Configuration;
using Shared.Services;
using Xunit;

namespace ResetPotentialWinLimit.Tests.Jobs
{
    public class ResetPotentialWinLimitJobTests
    {
        private readonly Mock<ILoggerService> _loggerServiceMock;
        private readonly Mock<IMaintenanceService> _maintenanceServiceMock;
        private readonly Mock<IResetPotentialWinLimitService> _resetPotentialWinLimitServiceMock;
        private readonly Mock<IOptions<JobsSettingsModel>> _settingsMock;
        private readonly UMClientConfiguration _umClientConfiguration;
        private readonly ScheduleJob[] _scheduleJobs;
        private readonly JobsSettingsModel _settings;

        public ResetPotentialWinLimitJobTests()
        {
            _loggerServiceMock = new Mock<ILoggerService>();
            _maintenanceServiceMock = new Mock<IMaintenanceService>();
            _resetPotentialWinLimitServiceMock = new Mock<IResetPotentialWinLimitService>();
            _settingsMock = new Mock<IOptions<JobsSettingsModel>>();
            _umClientConfiguration = new UMClientConfiguration();
            _scheduleJobs = new[] { new ScheduleJob { Name = "ResetPotentialWinLimitJob" } };
            _settings = new JobsSettingsModel
            {
                BatchSize = 100,
                DelayRetryInMilliSeconds = 1000
            };
            _settingsMock.Setup(x => x.Value).Returns(_settings);

            DateTimeExtentions.StubDateTime = new DateTime(2025, 6, 6, 23, 59, 58);
        }

        [Fact]
        public async Task ExecuteInternal_WhenJobHasCompletedToday_ShouldWaitForNextExecution()
        {
            // Arrange
            var jobStatus = new JobStatusModel { CompletedTime = DateTimeExtentions.Now };
            var jobConfig = new JobConfigModel();
            int getJobStatusCallCount = 0;
            int getJobConfigCallCount = 0;
            _resetPotentialWinLimitServiceMock.Setup(x => x.GetJobStatus())
                .Callback(() => getJobStatusCallCount++)
                .ReturnsAsync(jobStatus);
            _resetPotentialWinLimitServiceMock.Setup(x => x.GetJobConfig())
                .Callback(() => getJobConfigCallCount++)
                .ReturnsAsync(jobConfig);

            var job = CreateJob();

            // Act
            await job.ExecuteInternal(CancellationToken.None);

            // Assert
            Assert.Equal(1, getJobStatusCallCount);
            Assert.Equal(1, getJobConfigCallCount);
        }

        [Fact]
        public async Task ExecuteInternal_WhenNotRunningTime_ShouldWaitForNextExecution()
        {
            // Arrange
            var jobStatus = new JobStatusModel { CompletedTime = DateTimeExtentions.Now.AddDays(-1) };
            var jobConfig = new JobConfigModel { DefaultTime = "23:00" };
            int getJobStatusCallCount = 0;
            int getJobConfigCallCount = 0;
            _resetPotentialWinLimitServiceMock.Setup(x => x.GetJobStatus())
                .Callback(() => getJobStatusCallCount++)
                .ReturnsAsync(jobStatus);
            _resetPotentialWinLimitServiceMock.Setup(x => x.GetJobConfig())
                .Callback(() => getJobConfigCallCount++)
                .ReturnsAsync(jobConfig);

            var job = CreateJob();

            // Act
            await job.ExecuteInternal(CancellationToken.None);

            // Assert
            Assert.Equal(1, getJobStatusCallCount);
            Assert.Equal(1, getJobConfigCallCount);
        }

        [Fact]
        public async Task ExecuteInternal_WhenNoMembersToProcess_ShouldCompleteSuccessfully()
        {
            // Arrange
            var jobStatus = new JobStatusModel { CompletedTime = DateTimeExtentions.Now.AddDays(-1) };
            var jobConfig = new JobConfigModel { DefaultTime = "0:00" };
            var memberBatch = new MemberBatchModel { MemberIds = new List<int>() };

            _resetPotentialWinLimitServiceMock.Setup(x => x.GetJobStatus())
                .ReturnsAsync(jobStatus);
            _resetPotentialWinLimitServiceMock.Setup(x => x.GetJobConfig())
                .ReturnsAsync(jobConfig);
            _resetPotentialWinLimitServiceMock.Setup(x => x.SelectMemberList(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(memberBatch);

            int errorCallCount = 0;
            _loggerServiceMock.Setup(m => m.Error(It.IsAny<Exception>(), It.IsAny<ScheduleJob>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<Exception, ScheduleJob, string, string>((ex, job, methodName, message) => errorCallCount++);

            int updateJobStatusCallCount = 0;
            _resetPotentialWinLimitServiceMock.Setup(m => m.UpdateJobStatus(It.IsAny<JobStatusModel>()))
                .Callback(() => updateJobStatusCallCount++);

            var job = CreateJob();

            // Act
            await job.ExecuteInternal(CancellationToken.None);

            // Assert
            Assert.Equal(1, updateJobStatusCallCount);
            Assert.Equal(2, errorCallCount);
        }

        [Fact]
        public async Task ExecuteInternal_WhenProcessingBatchFails_ShouldStopExecution()
        {
            // Arrange
            var jobStatus = new JobStatusModel { CompletedTime = DateTimeExtentions.Now.AddDays(-1) };
            var jobConfig = new JobConfigModel { DefaultTime = "0:00" };
            var memberBatch = new MemberBatchModel { MemberIds = new List<int> { 1, 2, 3 } };

            _resetPotentialWinLimitServiceMock.Setup(x => x.GetJobStatus())
                .ReturnsAsync(jobStatus);
            _resetPotentialWinLimitServiceMock.Setup(x => x.GetJobConfig())
                .ReturnsAsync(jobConfig);
            _resetPotentialWinLimitServiceMock.Setup(x => x.SelectMemberList(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(memberBatch);

            int errorCallCount = 0;
            _loggerServiceMock.Setup(m => m.Error(It.IsAny<Exception>(), It.IsAny<ScheduleJob>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<Exception, ScheduleJob, string, string>((ex, job, methodName, message) => errorCallCount++);

            int processBatchCallCount = 0;
            _resetPotentialWinLimitServiceMock.Setup(m => m.ProcessBatch(It.IsAny<string>()))
                .Callback(() => processBatchCallCount++)
                .ReturnsAsync(new ResultModel { ErrCode = 1 });

            var job = CreateJob();

            // Act
            await job.ExecuteInternal(CancellationToken.None);

            // Assert
            Assert.Equal(5, processBatchCallCount);
            Assert.Equal(2, errorCallCount);
        }

        private ResetPotentialWinLimitJob CreateJob()
        {
            return new ResetPotentialWinLimitJob(
                _scheduleJobs,
                _loggerServiceMock.Object,
                _maintenanceServiceMock.Object,
                _umClientConfiguration,
                _resetPotentialWinLimitServiceMock.Object,
                _settingsMock.Object);
        }
    }
}