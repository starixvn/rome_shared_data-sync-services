using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class GetCashMemberListCriteriaTests
{
    [Fact]
    public void Constructor_ShouldSetIsTest()
    {
        // Arrange & Act
        var criteria = new GetCashMemberListCriteria(true);

        // Assert
        Assert.True(criteria.IsTest);
    }

    [Fact]
    public void Constructor_WithFalseParameter_ShouldSetIsTestToFalse()
    {
        // Arrange & Act
        var criteria = new GetCashMemberListCriteria(false);

        // Assert
        Assert.False(criteria.IsTest);
    }

    [Fact]
    public void CustIdList_ShouldBeSettable()
    {
        // Arrange
        var criteria = new GetCashMemberListCriteria(false);
        var expectedCustIdList = "123,456,789";

        // Act
        criteria.CustIdList = expectedCustIdList;

        // Assert
        Assert.Equal(expectedCustIdList, criteria.CustIdList);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnExpectedKey()
    {
        // Arrange
        var criteria = new GetCashMemberListCriteria(false);

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashService_AccumCreditReturn_YesterdayLoss_Sel", result);
    }

    [Fact]
    public void IsValid_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var criteria = new GetCashMemberListCriteria(false);

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.True(result);
    }
}