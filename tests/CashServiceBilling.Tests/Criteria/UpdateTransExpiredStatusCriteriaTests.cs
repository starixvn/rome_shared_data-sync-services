using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class UpdateTransExpiredStatusCriteriaTests
{
    [Fact]
    public void Constructor_SetsIsTestProperty()
    {
        // Arrange & Act
        var criteriaWithTestTrue = new UpdateTransExpiredStatusCriteria(true);
        var criteriaWithTestFalse = new UpdateTransExpiredStatusCriteria(false);

        // Assert
        Assert.True(criteriaWithTestTrue.IsTest);
        Assert.False(criteriaWithTestFalse.IsTest);
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectKey()
    {
        // Arrange
        var criteria = new UpdateTransExpiredStatusCriteria(false);

        // Act
        var key = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashService_UpdateExpiredStatus", key);
    }

    [Fact]
    public void IsValid_AlwaysReturnsTrue()
    {
        // Arrange
        var criteria = new UpdateTransExpiredStatusCriteria(false);

        // Act
        var isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }
}