using Xunit;
using ResetPotentialWinLimit.Criteria;

namespace ResetPotentialWinLimit.Tests.Criteria;

public class ProcessBatchCriteriaTests
{
    [Fact]
    public void Constructor_WhenValidCustListProvided_ShouldSetCustList()
    {
        // Arrange
        var expectedCustList = "123,456,789";

        // Act
        var criteria = new ProcessBatchCriteria(expectedCustList);

        // Assert
        Assert.Equal(expectedCustList, criteria.CustList);
    }

    [Fact]
    public void GetSettingKey_WhenCalled_ShouldReturnCorrectKey()
    {
        // Arrange
        var criteria = new ProcessBatchCriteria("123");

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Acc_CustWinLimitBet_Reset", result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_WhenCustListProvided_ShouldReturnExpectedResult(string custList, bool expectedResult)
    {
        // Arrange
        var criteria = new ProcessBatchCriteria(custList);

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.Equal(expectedResult, result);
    }
} 