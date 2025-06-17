using CashServiceBilling.Criteria;
using CashServiceBilling.Models;
using Xunit;

namespace CashServiceBilling.Tests.Criteria;

public class WriteLogCriteriaTests
{
    [Fact]
    public void Constructor_WhenInitializedWithValidParameters_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var siteDBGroupId = 1;
        var logModel = new LogModel { LogData = "test log data" };

        // Act
        var criteria = new WriteLogCriteria(siteDBGroupId, logModel);

        // Assert
        Assert.Equal(siteDBGroupId, criteria.SiteDBGroupId);
        Assert.Equal(logModel.LogData, criteria.LogData);
    }

    [Fact]
    public void GetSettingKey_WhenCalled_ShouldReturnCorrectKey()
    {
        // Arrange
        var siteDBGroupId = 1;
        var logModel = new LogModel { LogData = "test log data" };
        var criteria = new WriteLogCriteria(siteDBGroupId, logModel);
        var expectedKey = $"SiteDB_{siteDBGroupId}_WriteLog_CashService_UserLog_Transfer";

        // Act
        var actualKey = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expectedKey, actualKey);
    }

    [Fact]
    public void IsValid_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        var siteDBGroupId = 1;
        var logModel = new LogModel { LogData = "test log data" };
        var criteria = new WriteLogCriteria(siteDBGroupId, logModel);

        // Act
        var isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }
}