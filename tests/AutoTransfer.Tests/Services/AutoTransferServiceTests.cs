using AutoTransfer.Constants;
using AutoTransfer.Criteria;
using AutoTransfer.Models;
using AutoTransfer.Services;
using Fanex.Data.Repository;

namespace AutoTransfer.Tests.Services;

public class AutoTransferServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly ITransferService transferService;

    public AutoTransferServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        transferService = new TransferService(dynamicRepository);
    }

    [Fact]
    public async Task Start_Always_CallsExecuteAsync()
    {
        // Arrange
        int pairOfBatch = 200;
        string runningMode = RunningModeConstant.Normal;

        // Act
        await transferService.Start(pairOfBatch, runningMode);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<AutoTransferStartCriteria>(
            criteria => pairOfBatch == criteria.PairOfBatch && runningMode == criteria.RunningMode
        ));
    }

    [Fact]
    public async Task SelectBatches_Always_CallsFetchAsync()
    {
        // Arrange
        int numOfThread = 5;
        string runningMode = RunningModeConstant.Normal;

        // Act
        await transferService.SelectBatches(numOfThread, runningMode);

        // Assert
        await dynamicRepository.Received().FetchAsync<BatchModel>(Arg.Is<SelectBatchesCriteria>(
            criteria => numOfThread == criteria.NumOfThread && runningMode == criteria.RunningMode
        ));
    }

    [Fact]
    public async Task ProcessBatch_Always_CallsExecuteAsync()
    {
        // Arrange
        var batchModel = new BatchModel()
        {
            BatchNo = 1,
            FromCustId = 1,
            RunningMode = RunningModeConstant.Normal,
            ToCustIds = "1,2,3,4"
        };
        // Act
        await transferService.ProcessBatch(batchModel);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<ProcessBatchCriteria>(
             criteria => batchModel.BatchNo == criteria.BatchNo
             && batchModel.ToCustIds == criteria.ToCustIds
             && batchModel.RunningMode == criteria.RunningMode
             && batchModel.FromCustId == criteria.FromCustId
         ));
    }

    [Fact]
    public async Task SelectRetryBatches_Always_CallsFetchAsync()
    {
        // Arrange
        int numberOfThreads = 5;

        // Act
        await transferService.SelectRetryBatches(numberOfThreads);

        // Assert
        await dynamicRepository.Received().FetchAsync<BatchModel>(Arg.Is<SelectRetryBatchesCriteria>(
            criteria => numberOfThreads == criteria.NumOfThread
        ));
    }

    [Fact]
    public async Task ProcessRetryBatch_Always_CallsExecuteAsync()
    {
        // Arrange
        var batchModel = new BatchModel()
        {
            BatchNo = 1,
            FromCustId = 1,
            RunningMode = RunningModeConstant.Retry,
            ToCustIds = "1,2,3,4"
        };

        // Act
        await transferService.ProcessRetryBatch(batchModel);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<ProcessBatchCriteria>(
            criteria => batchModel.BatchNo == criteria.BatchNo
            && batchModel.ToCustIds == criteria.ToCustIds
            && batchModel.RunningMode == criteria.RunningMode
            && batchModel.FromCustId == criteria.FromCustId
        ));
    }

}
