using AutoTransfer.Constants;
using AutoTransfer.Criteria;
using AutoTransfer.Models;
using Xunit;

namespace AutoTransfer.Tests.Criteria;

public class ProcessBatchCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var batchModel = new BatchModel()
        {
            BatchNo = 1,
            FromCustId = 1,
            RunningMode = RunningModeConstant.Normal,
            ToCustIds = "1,2,3,4"
        };
        var criteria = new ProcessBatchCriteria(batchModel);

        // Act & Assert
        Assert.Equal("Age_ClearBalanceCust_Job_BatchProcess", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-100, 100, false)]
    [InlineData(-100, 0, false)]
    [InlineData(100, 0, false)]
    [InlineData(100, 100, true)]
    public void IsValid_ProcessBatchCriteria_ReturnsCorrectExpected(int batchNo, int fromCustId, bool expected)
    {
        // Arrange
        var batchModel = new BatchModel()
        {
            BatchNo = batchNo,
            FromCustId = fromCustId,
            RunningMode = RunningModeConstant.Normal,
            ToCustIds = "1,2,3,4"
        };

        // Act
        var criteria = new ProcessBatchCriteria(batchModel);

        // Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
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
        var criteria = new ProcessBatchCriteria(batchModel);

        // Assert
        Assert.True(
            1 == criteria.BatchNo
            && 1 == criteria.FromCustId
            && "1,2,3,4" == criteria.ToCustIds
            && RunningModeConstant.Normal == criteria.RunningMode

        );
    }

}
