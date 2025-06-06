using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Xunit;

namespace CashServiceBilling.Tests.Criteria;

public class StartJobCriteriaTests
{
    [Fact]
    public void Constructor_WhenValidBatchModelProvided_ShouldInitializeProperties()
    {
        // Arrange
        var batchModel = new BatchModel
        {
            SiteDBGroupId = 123,
            AgentList = "agent1,agent2"
        };

        // Act
        var criteria = new StartJobCriteria(batchModel);

        // Assert
        Assert.Equal(123, criteria.SiteDBGroupId);
        Assert.Equal("agent1,agent2", criteria.AgentList);
    }

    [Fact]
    public void GetSettingKey_WhenCalled_ShouldReturnCorrectKey()
    {
        // Arrange
        var batchModel = new BatchModel { SiteDBGroupId = 456 };
        var criteria = new StartJobCriteria(batchModel);

        // Act
        var settingKey = criteria.GetSettingKey();

        // Assert
        Assert.Equal("SiteDB_456_Age_CashServiceBilling_Job_Start", settingKey);
    }

    [Fact]
    public void IsValid_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        var batchModel = new BatchModel();
        var criteria = new StartJobCriteria(batchModel);

        // Act
        var isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }
}