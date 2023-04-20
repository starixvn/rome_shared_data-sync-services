using AutoTransfer.Constants;
using AutoTransfer.Criteria;

namespace AutoTransfer.Tests.Criteria;

public class SelectBatchesCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new SelectBatchCriteria(RunningModeConstant.Normal);

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal("Age_ClearBalanceCust_Job_BatchSel_PerfTest", actualValue);
    }

    [Fact]
    public void IsValid_Always_ReturnTrue()
    {
        // Arrange

        var criteria = new SelectBatchCriteria(RunningModeConstant.Normal);

        // Act
        var actualValue = criteria.IsValid();

        // Assert
        Assert.True(actualValue);
    }
}
