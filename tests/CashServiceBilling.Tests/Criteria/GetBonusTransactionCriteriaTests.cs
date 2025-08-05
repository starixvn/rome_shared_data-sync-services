using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class GetBonusTransactionCriteriaTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        int siteDBGroupId = 1;
        int maxCustId = 100;
        DateTime transDate = new DateTime(2023, 1, 1);

        // Act
        var criteria = new GetBonusTransactionCriteria(siteDBGroupId, maxCustId, transDate);

        // Assert
        Assert.Equal(siteDBGroupId, criteria.SiteDBGroupId);
        Assert.Equal(maxCustId, criteria.MaxCustId);
        Assert.Equal(transDate, criteria.TransDate);
        Assert.Null(criteria.MemberBonus);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var criteria = new GetBonusTransactionCriteria(1, 100, DateTime.Now);

        // Act
        string key = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashService_BonusTransaction_Sel", key);
    }

    [Fact]
    public void IsValid_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var criteria = new GetBonusTransactionCriteria(1, 100, DateTime.Now);

        // Act
        bool isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void MemberBonus_ShouldSetAndGetValue()
    {
        // Arrange
        var criteria = new GetBonusTransactionCriteria(1, 100, DateTime.Now);
        string expectedMemberBonus = "TestBonus";

        // Act
        criteria.MemberBonus = expectedMemberBonus;

        // Assert
        Assert.Equal(expectedMemberBonus, criteria.MemberBonus);
    }

    [Fact]
    public void MaxCustId_ShouldSetAndGetValue()
    {
        // Arrange
        var criteria = new GetBonusTransactionCriteria(1, 100, DateTime.Now);
        int expectedMaxCustId = 200;

        // Act
        criteria.MaxCustId = expectedMaxCustId;

        // Assert
        Assert.Equal(expectedMaxCustId, criteria.MaxCustId);
    }
}
