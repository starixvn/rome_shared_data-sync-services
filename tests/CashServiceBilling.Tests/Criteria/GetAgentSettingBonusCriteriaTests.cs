namespace CashServiceBilling.Criteria.Tests;

public class GetAgentSettingBonusCriteriaTests
{
    [Fact]
    public void Constructor_InitializesProperties_Correctly()
    {
        // Arrange
        bool isTest = true;
        DateTime mondayLastWeek = new DateTime(2023, 6, 5);
        DateTime sundayLastWeek = new DateTime(2023, 6, 11);

        // Act
        var criteria = new GetAgentSettingBonusCriteria(isTest, mondayLastWeek, sundayLastWeek);

        // Assert
        Assert.Equal(isTest, criteria.IsTest);
        Assert.Equal(mondayLastWeek, criteria.MondayLastWeek);
        Assert.Equal(sundayLastWeek, criteria.SundayLastWeek);
        Assert.Null(criteria.AgentPromotion);
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectKey()
    {
        // Arrange
        var criteria = new GetAgentSettingBonusCriteria(false, DateTime.Now, DateTime.Now);

        // Act
        string result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashService_AgentSetting_Sel", result);
    }

    [Fact]
    public void IsValid_AlwaysReturnsTrue()
    {
        // Arrange
        var criteria = new GetAgentSettingBonusCriteria(false, DateTime.Now, DateTime.Now);

        // Act
        bool result = criteria.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AgentPromotion_CanBeSetAndRetrieved()
    {
        // Arrange
        var criteria = new GetAgentSettingBonusCriteria(false, DateTime.Now, DateTime.Now);
        string expectedPromotion = "TestPromotion";

        // Act
        criteria.AgentPromotion = expectedPromotion;

        // Assert
        Assert.Equal(expectedPromotion, criteria.AgentPromotion);
    }
}
