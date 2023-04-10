using AutoTransfer.Constants;
using AutoTransfer.Criteria;
using AutoTransfer.Models;
using Xunit;

namespace AutoTransfer.Tests.Criteria;

public class SelectBatchesCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new SelectBatchesCriteria(1, RunningModeConstant.Normal);

        // Act & Assert
        Assert.Equal("Age_ClearBalanceCust_Job_BatchSel", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_SelectBatchesCriteria_ReturnsCorrectExpected(int numberOfThreads, bool expected)
    {
        // Arrange

        var criteria = new SelectBatchesCriteria(numberOfThreads, RunningModeConstant.Normal);

        // Act & Assert

        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange

        var criteria = new SelectBatchesCriteria(1, RunningModeConstant.Normal);

        // Act & Assert
        Assert.True(
            1 == criteria.NumOfThread
            && RunningModeConstant.Normal == criteria.RunningMode

        );
    }

}
