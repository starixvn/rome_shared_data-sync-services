using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class WriteLogRunSPCriteriaTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        int userId = 123;
        int custId = 456;
        int actionId = 789;
        string trackingValueMask = "mask1";
        string trackingValueMask2 = "mask2";
        string trackingValue = "value1";
        string trackingValue2 = "value2";
        short custDBGroupId = 10;

        // Act
        var criteria = new WriteLogRunSPCriteria(
            userId,
            custId,
            actionId,
            trackingValueMask,
            trackingValueMask2,
            trackingValue,
            trackingValue2,
            custDBGroupId);

        // Assert
        Assert.Equal(userId, criteria.UserId);
        Assert.Equal(custId, criteria.CustId);
        Assert.Equal(actionId, criteria.ActionId);
        Assert.Equal(trackingValueMask, criteria.TrackingValueMask);
        Assert.Equal(trackingValueMask2, criteria.TrackingValueMask2);
        Assert.Equal(trackingValue, criteria.TrackingValue);
        Assert.Equal(trackingValue2, criteria.TrackingValue2);
        Assert.Equal(custDBGroupId, criteria.CustDBGroupId);
    }

    [Fact]
    public void GetSettingKey_WithCustDBGroupId_ShouldReturnCustomKey()
    {
        // Arrange
        var criteria = new WriteLogRunSPCriteria(1, 1, 1, "", "", "", "", 5);

        // Act
        string settingKey = criteria.GetSettingKey();

        // Assert
        Assert.Equal("CustDB_5_WriteLog_CashService_UserLog_RunSP", settingKey);
    }

    [Fact]
    public void GetSettingKey_WithoutCustDBGroupId_ShouldReturnDefaultKey()
    {
        // Arrange
        var criteria = new WriteLogRunSPCriteria(1, 1, 1, "", "", "", "", null);

        // Act
        string settingKey = criteria.GetSettingKey();

        // Assert
        Assert.Equal("WriteLog_CashService_UserLog_RunSP", settingKey);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValid_ShouldReturnExpectedResult(int custId, bool expectedResult)
    {
        // Arrange
        var criteria = new WriteLogRunSPCriteria(1, custId, 1, "", "", "", "");

        // Act
        bool isValid = criteria.IsValid();

        // Assert
        Assert.Equal(expectedResult, isValid);
    }
}
