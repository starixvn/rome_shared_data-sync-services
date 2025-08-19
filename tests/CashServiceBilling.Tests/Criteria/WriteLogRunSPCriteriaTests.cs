using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class WriteLogRunSPCriteriaTests
{
    [Fact]
    public void Constructor_ShouldSetProperties_WhenCalled()
    {
        // Arrange
        var logData = "log data";
        short custDBGroupId = 1;

        // Act
        var criteria = new WriteLogRunSPCriteria(logData) { CustDBGroupId = custDBGroupId };

        // Assert
        Assert.Equal(logData, criteria.LogData_Multiple);
        Assert.Equal(custDBGroupId, criteria.CustDBGroupId);
    }

    [Fact]
    public void Constructor_ShouldSetNullCustDBGroupId_WhenNotProvided()
    {
        // Arrange
        var logData = "log data";

        // Act
        var criteria = new WriteLogRunSPCriteria(logData);

        // Assert
        Assert.Equal(logData, criteria.LogData_Multiple);
        Assert.Null(criteria.CustDBGroupId);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnCorrectKey_WhenCustDBGroupIdHasValue()
    {
        // Arrange
        var criteria = new WriteLogRunSPCriteria("log data")
        {
            CustDBGroupId = 1
        };
        var expectedKey = "CustDB_1_WriteLog_CashService_UserLog_RunSP";

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expectedKey, result);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnDefaultKey_WhenCustDBGroupIdIsNull()
    {
        // Arrange
        var criteria = new WriteLogRunSPCriteria("log data");
        var expectedKey = "WriteLog_CashService_UserLog_RunSP";

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expectedKey, result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenLogDataIsProvided()
    {
        // Arrange
        var criteria = new WriteLogRunSPCriteria("log data");

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_ShouldReturnFalse_WhenLogDataIsNullOrWhitespace(string logData)
    {
        // Arrange
        var criteria = new WriteLogRunSPCriteria(logData);

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.False(result);
    }
}
