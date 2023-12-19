using AutoTransfer.Constants;
using AutoTransfer.Criteria;

namespace AutoTransfer.Tests.Criteria;

public class AutoTransferStartCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new AutoTransferStartCriteria(200, RunningModeConstant.Normal, "");

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal("Age_ClearBalanceCust_Job_Start", actualValue);
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_AutoTransferStartCriteria_ReturnsCorrectExpected(int pairOfBatch, bool expected)
    {
        // Arrange
        var criteria = new AutoTransferStartCriteria(pairOfBatch, RunningModeConstant.Normal, "");

        // Act
        var actualValue = criteria.IsValid();

        //Assert
        Assert.Equal(expected, actualValue);
    }
}
