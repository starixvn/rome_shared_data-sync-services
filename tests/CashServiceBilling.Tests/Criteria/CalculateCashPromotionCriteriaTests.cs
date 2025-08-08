using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class CalculateCashPromotionCriteriaTests
{
    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        // Arrange
        var productDetails = "Product123";
        var moveBetsDay = DateTime.Today;
        var balanceUpDay = DateTime.Today.AddDays(1);
        var agentPromotion = "Promo123";
        var transDate = DateTime.Today.AddDays(-1);

        // Act
        var criteria = new CalculateCashPromotionCriteria(
            productDetails,
            moveBetsDay,
            balanceUpDay,
            agentPromotion,
            transDate);

        // Assert
        Assert.Equal(productDetails, criteria.ProductDetails);
        Assert.Equal(moveBetsDay, criteria.MoveBetsDay);
        Assert.Equal(balanceUpDay, criteria.BalanceUpDay);
        Assert.Equal(agentPromotion, criteria.AgentPromotion);
        Assert.Equal(transDate, criteria.TransDate);
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectKey()
    {
        // Arrange
        var criteria = new CalculateCashPromotionCriteria(
            "Product123",
            DateTime.Today,
            DateTime.Today,
            "Promo123",
            DateTime.Today);

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashService_CashPromotionCal", result);
    }

    [Theory]
    [InlineData("Promo123", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    public void IsValid_ReturnsExpectedResult(string agentPromotion, bool expected)
    {
        // Arrange
        var criteria = new CalculateCashPromotionCriteria(
            "Product123",
            DateTime.Today,
            DateTime.Today,
            agentPromotion,
            DateTime.Today);

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.Equal(expected, result);
    }
}
