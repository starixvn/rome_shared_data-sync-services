using AutoTransfer.Constants;
using AutoTransfer.Criteria;
using AutoTransfer.Models;
using AutoTransfer.Services;
using Fanex.Data.Repository;
using KellermanSoftware.CompareNetObjects;

namespace AutoTransfer.Tests.Services;

public class AutoTransferServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly TransferService transferService;
    private readonly CompareLogic compareLogic;

    public AutoTransferServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        transferService = new TransferService(dynamicRepository);
        compareLogic = new CompareLogic();
    }

    [Fact]
    public async Task Start_Always_ReturnsCorrectValue()
    {
        // Arrange
        int pairOfBatch = 200;
        string runningMode = RunningModeConstant.Normal;
        var expectedModel = new AutoTransferStartModel
        {
            TotalPair = 0,
            TotalBatch = 0,
            TotalSuper = 0,
            RunningMode = RunningModeConstant.Normal,
            ErrCode = 0,
            ErrMsg = null
        };

        // Act
        var actualValue = await transferService.Start(pairOfBatch, runningMode);

        // Assert
        Assert.True(compareLogic.Compare(expectedModel, actualValue).AreEqual);
    }

    [Fact]
    public async Task SelectBatch_Always_ReturnsCorrectValue()
    {
        // Arrange
        string runningMode = RunningModeConstant.Normal;
        var expectedModel = new BatchModel();
        dynamicRepository.FetchAsync<BatchModel>(Arg.Any<SelectBatchCriteria>()).Returns(new List<BatchModel>());

        // Act
        var actualValue = await transferService.SelectBatch(runningMode);

        // Assert
        Assert.True(compareLogic.Compare(expectedModel, actualValue).AreEqual);
    }

    [Fact]
    public async Task ProcessBatch_Always_ReturnsCorrectValue()
    {
        // Arrange
        var batchModel = new BatchModel()
        {
            BatchNo = 1,
            FromCustId = 1,
            RunningMode = RunningModeConstant.Normal,
            ToCustIds = "1,2,3,4"
        };

        var expectedModel = new ResultModel();

        // Act
        var actualValue = await transferService.ProcessBatch(batchModel);

        // Assert
        Assert.True(compareLogic.Compare(expectedModel, actualValue).AreEqual);
    }

    [Fact]
    public async Task SelectRetryBatches_Always_ReturnsCorrectValue()
    {
        // Arrange
        int numberOfThreads = 5;
        dynamicRepository.FetchAsync<BatchModel>(Arg.Any<SelectRetryBatchesCriteria>())
           .Returns(new List<BatchModel>());
        var expectedModel = new RetryBatchesModel
        {
            Batches = new List<BatchModel>(),
            IsCompleted = false
        };

        // Act
        var actualValue = await transferService.SelectRetryBatches(numberOfThreads);

        // Assert
        Assert.True(compareLogic.Compare(expectedModel, actualValue).AreEqual);
    }

    [Fact]
    public async Task ProcessRetryBatch_Always_ReturnsCorrectValue()
    {
        // Arrange
        var batchModel = new BatchModel()
        {
            BatchNo = 1,
            FromCustId = 1,
            RunningMode = RunningModeConstant.Retry,
            ToCustIds = "1,2,3,4"
        };

        var expectedModel = new ResultModel();

        // Act
        var actualValue = await transferService.ProcessRetryBatch(batchModel);


        // Assert
        Assert.True(compareLogic.Compare(expectedModel, actualValue).AreEqual);
    }

    [Fact]
    public async Task CheckBalanceUpProcess_Always_ReturnsCorrectValue()
    {
        // Act
        var actualValue = await transferService.CheckBalanceUpProcess();


        // Assert
        Assert.Null(actualValue);
    }

    [Fact]
    public async Task ImportLogs_AllLogSuccess_ReturnsCorrectValue()
    {
        // Arrange 
        dynamicRepository.FetchAsync<AutoTransferLogModel>(Arg.Any<ImportMonitorLogCriteria>())
            .Returns(new List<AutoTransferLogModel>{
                new AutoTransferLogModel { RowData = "1" },
                new AutoTransferLogModel { RowData = "2" },
            });

        var expectedModel = new ResultModel
        {
            ErrCode = 0,
            ErrMsg = string.Empty
        };

        // Act
        var actualValue = await transferService.ImportLogs(1);

        // Assert
        Assert.True(compareLogic.Compare(expectedModel, actualValue).AreEqual);
    }

    [Fact]
    public void ImportLogs_HasFailedLog_ReturnsCorrectValue()
    {
        // Arrange 
        dynamicRepository.FetchAsync<AutoTransferLogModel>(Arg.Any<ImportMonitorLogCriteria>())
            .Returns(new List<AutoTransferLogModel>{
                new AutoTransferLogModel { RowData = "1" },
                new AutoTransferLogModel { RowData = "2" },
                new AutoTransferLogModel { RowData = "1" },
            });

        dynamicRepository.When(x => x.ExecuteAsync(Arg.Is<ImportHistoryLogCriteria>(c => c.RowData == "1")))
            .Do(x =>
            {
                var criteria = (ImportHistoryLogCriteria)x[0];
                if (criteria != null)
                {
                    criteria.ErrCode = 1;
                    criteria.ErrMsg = "Error " + criteria.RowData;
                }
            });

        var expectedModel = new ResultModel
        {
            ErrCode = 1,
            ErrMsg = "Error 1\n\rError 1\n\rError 1\n\rError 1"
        };

        // Act
        var actualValue = transferService.ImportLogs(1).GetAwaiter().GetResult();

        // Assert
        Assert.True(compareLogic.Compare(expectedModel, actualValue).AreEqual);
    }
}
