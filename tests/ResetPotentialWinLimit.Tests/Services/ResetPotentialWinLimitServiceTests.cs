using Fanex.Data.Repository;
using Moq;
using Newtonsoft.Json;
using ResetPotentialWinLimit.Criteria;
using ResetPotentialWinLimit.Extensions;
using ResetPotentialWinLimit.Models;
using ResetPotentialWinLimit.Services;
using Xunit;

namespace ResetPotentialWinLimit.Tests.Services;

public class ResetPotentialWinLimitServiceTests
{
    private readonly Mock<IDynamicRepository> _mockDynamicRepository;
    private readonly IResetPotentialWinLimitService _service;

    public ResetPotentialWinLimitServiceTests()
    {
        _mockDynamicRepository = new Mock<IDynamicRepository>();
        _service = new ResetPotentialWinLimitService(_mockDynamicRepository.Object);
        DateTimeExtentions.StubDateTime = new DateTime(2025, 6, 6, 10, 1, 0);
    }

    [Fact]
    public async Task GetJobConfig_WhenConfigExists_ReturnsDeserializedConfig()
    {
        // Arrange
        var expectedConfig = new JobConfigModel
        {
            DefaultTime = "10:00",
            IsActive = true,
            ManualTime = DateTimeExtentions.Now
        };
        var criteria = new GetJobStatusCommand("ResetPotentialWinLimitJob_Config");
        _mockDynamicRepository
            .Setup(x => x.FetchAsync<JobConfigModel>(It.IsAny<GetJobStatusCommand>()))
            .Callback<ICriteria>(cmd => (cmd as GetJobStatusCommand).VValue = JsonConvert.SerializeObject(expectedConfig));

        // Act
        var result = await _service.GetJobConfig();

        // Assert
        Assert.Equal(expectedConfig.DefaultTime, result.DefaultTime);
        Assert.Equal(expectedConfig.IsActive, result.IsActive);
        Assert.Equal(expectedConfig.ManualTime, result.ManualTime);
        _mockDynamicRepository.Verify(x => x.FetchAsync<JobConfigModel>(It.IsAny<GetJobStatusCommand>()), Times.Once);
    }

    [Fact]
    public async Task GetJobConfig_WhenConfigDoesNotExist_ReturnsDefaultConfig()
    {
        // Arrange
        var criteria = new GetJobStatusCommand("ResetPotentialWinLimitJob_Config");
        _mockDynamicRepository
            .Setup(x => x.FetchAsync<JobConfigModel>(It.IsAny<GetJobStatusCommand>()))
            .Callback<ICriteria>(cmd => (cmd as GetJobStatusCommand).VValue = null);

        // Act
        var result = await _service.GetJobConfig();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0:00", result.DefaultTime);
        Assert.True(result.IsActive);
        Assert.Null(result.ManualTime);
        _mockDynamicRepository.Verify(x => x.FetchAsync<JobConfigModel>(It.IsAny<GetJobStatusCommand>()), Times.Once);
    }

    [Fact]
    public async Task GetJobStatus_WhenStatusExists_ReturnsDeserializedStatus()
    {
        // Arrange
        var expectedStatus = new JobStatusModel
        {
            RunningDate = DateTimeExtentions.Now,
            LastMaxCustID = 1000,
            CompletedTime = DateTimeExtentions.Now
        };
        var criteria = new GetJobStatusCommand("ResetPotentialWinLimitJob_Status");
        _mockDynamicRepository
            .Setup(x => x.FetchAsync<JobStatusModel>(It.IsAny<GetJobStatusCommand>()))
            .Callback<ICriteria>(cmd => (cmd as GetJobStatusCommand).VValue = JsonConvert.SerializeObject(expectedStatus));

        // Act
        var result = await _service.GetJobStatus();

        // Assert
        Assert.Equal(expectedStatus.RunningDate, result.RunningDate);
        Assert.Equal(expectedStatus.LastMaxCustID, result.LastMaxCustID);
        Assert.Equal(expectedStatus.CompletedTime, result.CompletedTime);
        _mockDynamicRepository.Verify(x => x.FetchAsync<JobStatusModel>(It.IsAny<GetJobStatusCommand>()), Times.Once);
    }

    [Fact]
    public async Task GetJobStatus_WhenStatusDoesNotExist_ReturnsDefaultStatus()
    {
        // Arrange
        var criteria = new GetJobStatusCommand("ResetPotentialWinLimitJob_Status");
        _mockDynamicRepository
            .Setup(x => x.FetchAsync<JobStatusModel>(It.IsAny<GetJobStatusCommand>()))
            .Callback<ICriteria>(cmd => (cmd as GetJobStatusCommand).VValue = null);

        // Act
        var result = await _service.GetJobStatus();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.RunningDate);
        Assert.Equal(0, result.LastMaxCustID);
        Assert.Null(result.CompletedTime);
        _mockDynamicRepository.Verify(x => x.FetchAsync<JobStatusModel>(It.IsAny<GetJobStatusCommand>()), Times.Once);
    }

    [Fact]
    public async Task UpdateJobStatus_WhenCalled_ExecutesUpdateCommand()
    {
        // Arrange
        var jobStatus = new JobStatusModel
        {
            RunningDate = DateTimeExtentions.Now,
            LastMaxCustID = 1000,
            CompletedTime = DateTimeExtentions.Now
        };
        var expectedJson = JsonConvert.SerializeObject(jobStatus);

        // Act
        await _service.UpdateJobStatus(jobStatus);

        // Assert
        _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.Is<UpdateJobStatusCommand>(
            cmd => cmd.VValue == expectedJson && cmd.VName == "ResetPotentialWinLimitJob_Status")), Times.Once);
    }

    [Fact]
    public async Task SelectMemberList_WhenCalled_ReturnsMemberBatch()
    {
        // Arrange
        var batchSize = 100;
        var lastMaxCustID = 1000;
        var expectedMemberIds = new List<int> { 1, 2, 3 };
        _mockDynamicRepository
            .Setup(x => x.FetchAsync<int>(It.IsAny<SelectMemberListCriteria>()))
            .ReturnsAsync(expectedMemberIds);

        // Act
        var result = await _service.SelectMemberList(batchSize, lastMaxCustID);

        // Assert
        Assert.Equal(expectedMemberIds, result.MemberIds);
        Assert.Equal(lastMaxCustID, result.LastMaxCustID);
        _mockDynamicRepository.Verify(x => x.FetchAsync<int>(It.IsAny<SelectMemberListCriteria>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatch_WhenSuccessful_ReturnsSuccessResult()
    {
        // Arrange
        var custList = "1,2,3";

        // Act
        var result = await _service.ProcessBatch(custList);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.Null(result.ErrMsg);
        _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.IsAny<ProcessBatchCriteria>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatch_WhenExceptionOccurs_ReturnsErrorResult()
    {
        // Arrange
        var custList = "1,2,3";
        var expectedError = "Test error";
        _mockDynamicRepository
            .Setup(x => x.ExecuteAsync(It.IsAny<ProcessBatchCriteria>()))
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var result = await _service.ProcessBatch(custList);

        // Assert
        Assert.Equal(1, result.ErrCode);
        Assert.Equal(expectedError, result.ErrMsg);
        _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.IsAny<ProcessBatchCriteria>()), Times.Once);
    }
}