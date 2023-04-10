using AutoTransfer.Constants;
using AutoTransfer.Criteria;
using Xunit;

namespace AutoTransfer.Tests.Criteria;

public class AutoTransferStartCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new AutoTransferStartCriteria(200, RunningModeConstant.Normal);

        // Act & Assert
        Assert.Equal("Age_ClearBalanceCust_Job_Start", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_AutoTransferStartCriteria_ReturnsCorrectExpected(int pairOfBatch, bool expected)
    {
        // Arrange
        var criteria = new AutoTransferStartCriteria(pairOfBatch, RunningModeConstant.Normal);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange
        int pairOfBatch = 100;
        string runningMode = RunningModeConstant.Normal;

        // Act 
        var criteria = new AutoTransferStartCriteria(pairOfBatch, runningMode);

        // Assert
        Assert.Equal(pairOfBatch, criteria.PairOfBatch);
        Assert.Equal(runningMode, criteria.RunningMode);
    }

}
