using AutoTransfer.Constants;
using AutoTransfer.Criteria;

namespace AutoTransfer.Tests.Criteria;

public class SelectBatchesCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new SelectBatchesCriteria(1, RunningModeConstant.Normal);

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal("Age_ClearBalanceCust_Job_BatchSel", actualValue);
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_SelectBatchesCriteria_ReturnsCorrectExpected(int numberOfThreads, bool expected)
    {
        // Arrange

        var criteria = new SelectBatchesCriteria(numberOfThreads, RunningModeConstant.Normal);

        // Act
        var actualValue = criteria.IsValid();

        // Assert
        Assert.Equal(expected, actualValue);
    }
}
