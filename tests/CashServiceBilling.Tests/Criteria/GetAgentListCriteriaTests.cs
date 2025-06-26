using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class GetAgentListCriteriaTests
{
    [Fact]
    public void Constructor_WhenGivenSiteDBGroupIds_ShouldJoinWithComma()
    {
        // Arrange
        var siteDBGroupIds = new[] { 1, 2, 3 };

        // Act
        var criteria = new GetAgentListCriteria(siteDBGroupIds);

        // Assert
        Assert.Equal("1,2,3", criteria.SiteDBGroupIds);
    }

    [Fact]
    public void GetSettingKey_WhenCalled_ShouldReturnCorrectKey()
    {
        // Arrange
        var criteria = new GetAgentListCriteria(new[] { 1 });

        // Act
        var settingKey = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_CashServiceBilling_AgentSel", settingKey);
    }

    [Fact]
    public void IsValid_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        var criteria = new GetAgentListCriteria(new[] { 1 });

        // Act
        var isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }
}