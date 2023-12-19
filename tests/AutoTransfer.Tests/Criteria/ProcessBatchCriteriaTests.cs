using AutoTransfer.Constants;
using AutoTransfer.Criteria;
using AutoTransfer.Models;

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
        var criteria = new ProcessBatchCriteria(batchModel, string.Empty);

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal("Age_ClearBalanceCust_Job_BatchProcess", actualValue);
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
        var criteria = new ProcessBatchCriteria(batchModel, string.Empty);

        // Act
        var actualValue = criteria.IsValid();

        // Assert
        Assert.Equal(expected, actualValue);
    }
}
