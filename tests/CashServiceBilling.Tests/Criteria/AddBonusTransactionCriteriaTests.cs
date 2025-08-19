using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class AddBonusTransactionCriteriaTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        string memberBonus = "BONUS123";
        int siteDbGroupId = 5;
        DateTime transDate = new DateTime(2023, 1, 1);

        // Act
        var criteria = new AddBonusTransactionCriteria(memberBonus, siteDbGroupId, transDate);

        // Assert
        Assert.Equal(memberBonus, criteria.MemberBonus);
        Assert.Equal(siteDbGroupId, criteria.SiteDBGroupId);
        Assert.Equal(transDate, criteria.TransDate);
        Assert.False(criteria.IsBonusChanged);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        int siteDbGroupId = 10;
        var criteria = new AddBonusTransactionCriteria("BONUS123", siteDbGroupId, DateTime.Now);

        // Act
        string key = criteria.GetSettingKey();

        // Assert
        Assert.Equal($"SiteDB_{siteDbGroupId}_Task_CashService_BonusTransaction_Ins", key);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenMemberBonusIsNotEmpty()
    {
        // Arrange
        var criteria = new AddBonusTransactionCriteria("BONUS123", 1, DateTime.Now);

        // Act
        bool isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_ShouldReturnFalse_WhenMemberBonusIsNullOrWhitespace(string memberBonus)
    {
        // Arrange
        var criteria = new AddBonusTransactionCriteria(memberBonus, 1, DateTime.Now);

        // Act
        bool isValid = criteria.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsBonusChanged_ShouldBeSettable()
    {
        // Arrange
        var criteria = new AddBonusTransactionCriteria("BONUS123", 1, DateTime.Now);

        // Act
        criteria.IsBonusChanged = true;

        // Assert
        Assert.True(criteria.IsBonusChanged);
    }
}
