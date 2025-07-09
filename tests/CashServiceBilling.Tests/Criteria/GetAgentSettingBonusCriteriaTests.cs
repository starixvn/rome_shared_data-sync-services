using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class GetAgentSettingBonusCriteriaTests
{
    [Fact]
    public void Constructor_ShouldSetIsTestCorrectly()
    {
        // Arrange & Act
        var criteriaTrue = new GetAgentSettingBonusCriteria(true);
        var criteriaFalse = new GetAgentSettingBonusCriteria(false);

        // Assert
        Assert.True(criteriaTrue.IsTest);
        Assert.False(criteriaFalse.IsTest);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var criteria = new GetAgentSettingBonusCriteria(false);

        // Act
        string result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashService_AgentSetting_Sel", result);
    }

    [Fact]
    public void IsValid_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var criteria = new GetAgentSettingBonusCriteria(false);

        // Act
        bool result = criteria.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AgentPromotion_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var criteria = new GetAgentSettingBonusCriteria(false);
        string expected = "TestPromotion";

        // Act
        criteria.AgentPromotion = expected;

        // Assert
        Assert.Equal(expected, criteria.AgentPromotion);
    }
}