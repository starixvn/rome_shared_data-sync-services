using AutoTransfer.Criteria;

namespace AutoTransfer.Tests.Criteria;

public class CheckBalanceUpProcessCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new CheckBalanceUpProcessCriteria();

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal("Acc_CheckProcessingData", actualValue);
    }

    [Fact]
    public void IsValid_Always_ReturnsTrue()
    {
        // Arrange
        var criteria = new CheckBalanceUpProcessCriteria();

        // Act
        var actualValue = criteria.IsValid();

        //Assert
        Assert.True(actualValue);
    }
}
