using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class UpdatePromotionExpiredStatusCriteriaTests
{
    [Fact]
    public void Constructor_SetsIsTestProperty()
    {
        // Arrange & Act
        var criteria = new UpdatePromotionExpiredStatusCriteria(true);

        // Assert
        Assert.True(criteria.IsTest);
    }

    [Fact]
    public void Constructor_WithFalseParameter_SetsIsTestPropertyToFalse()
    {
        // Arrange & Act
        var criteria = new UpdatePromotionExpiredStatusCriteria(false);

        // Assert
        Assert.False(criteria.IsTest);
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectKey()
    {
        // Arrange
        var criteria = new UpdatePromotionExpiredStatusCriteria(false);

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashPromotion_UpdateExpiredStatus", result);
    }

    [Fact]
    public void IsValid_ReturnsTrue()
    {
        // Arrange
        var criteria = new UpdatePromotionExpiredStatusCriteria(false);

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.True(result);
    }
}