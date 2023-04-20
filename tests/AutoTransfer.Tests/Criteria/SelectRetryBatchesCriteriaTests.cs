using AutoTransfer.Constants;
using AutoTransfer.Criteria;
using AutoTransfer.Models;
using Xunit;

namespace AutoTransfer.Tests.Criteria;

public class SelectRetryBatchesCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new SelectRetryBatchesCriteria(1);

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal("Age_ClearBalanceCust_Job_ReTrySel", actualValue);
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_SelectRetryBatchesCriteria_ReturnsCorrectExpected(int numberOfThreads, bool expected)
    {
        // Arrange

        var criteria = new SelectRetryBatchesCriteria(numberOfThreads);

        // Act
        var actualValue = criteria.IsValid();

        // Assert
        Assert.Equal(expected, actualValue);
    }

}
