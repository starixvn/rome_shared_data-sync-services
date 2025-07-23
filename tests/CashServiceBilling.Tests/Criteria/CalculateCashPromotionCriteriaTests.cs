using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class CalculateCashPromotionCriteriaTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var productDetails = "Product123";
        var moveBetsDay = new DateTime(2023, 1, 15);
        var balanceUpDay = new DateTime(2023, 1, 20);
        var agentPromotion = "Promotion456";

        // Act
        var criteria = new CalculateCashPromotionCriteria(
            productDetails,
            moveBetsDay,
            balanceUpDay,
            agentPromotion);

        // Assert
        Assert.Equal(productDetails, criteria.ProductDetails);
        Assert.Equal(moveBetsDay, criteria.MoveBetsDay);
        Assert.Equal(balanceUpDay, criteria.BalanceUpDay);
        Assert.Equal(agentPromotion, criteria.AgentPromotion);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var criteria = new CalculateCashPromotionCriteria(
            "Product123",
            DateTime.Now,
            DateTime.Now,
            "Promotion456");

        // Act
        var key = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashService_CashPromotionCal", key);
    }

    [Theory]
    [InlineData("ValidPromotion", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    public void IsValid_ShouldReturnCorrectResult(string agentPromotion, bool expected)
    {
        // Arrange
        var criteria = new CalculateCashPromotionCriteria(
            "Product123",
            DateTime.Now,
            DateTime.Now,
            agentPromotion);

        // Act
        var isValid = criteria.IsValid();

        // Assert
        Assert.Equal(expected, isValid);
    }
}
