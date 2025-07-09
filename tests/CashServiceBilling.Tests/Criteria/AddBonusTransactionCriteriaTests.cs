
using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class AddBonusTransactionCriteriaTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        string agentPromotion = "testPromotion";
        short siteDBGroupId = 1;

        // Act
        var criteria = new AddBonusTransactionCriteria(agentPromotion, siteDBGroupId);

        // Assert
        Assert.Equal(agentPromotion, criteria.AgentPromotion);
        Assert.Equal(siteDBGroupId, criteria.SiteDBGroupId);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var criteria = new AddBonusTransactionCriteria("testPromotion", 2);
        string expected = "SiteDB_2_Task_CashService_BonusTransaction_Ins";

        // Act
        string actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("validPromotion", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    public void IsValid_ShouldValidateAgentPromotion(string agentPromotion, bool expected)
    {
        // Arrange
        var criteria = new AddBonusTransactionCriteria(agentPromotion, 1);

        // Act
        bool actual = criteria.IsValid();

        // Assert
        Assert.Equal(expected, actual);
    }
}