using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class AddManualDepositTransCriteriaTests
{
    [Fact]
    public void Constructor_ShouldSetCustIdList()
    {
        // Arrange
        var custIdList = "123,456,789";

        // Act
        var criteria = new AddManualDepositTransCriteria(custIdList);

        // Assert
        Assert.Equal(custIdList, criteria.CustIdList);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnExpectedValue()
    {
        // Arrange
        var criteria = new AddManualDepositTransCriteria("123");

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_CashService_AccumCreditReturn_YesterdayLoss_Adjust", result);
    }

    [Theory]
    [InlineData("123,456", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    public void IsValid_ShouldReturnExpectedResult(string custIdList, bool expected)
    {
        // Arrange
        var criteria = new AddManualDepositTransCriteria(custIdList);

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CashAccumCreditTbl_ShouldBeSettable()
    {
        // Arrange
        var criteria = new AddManualDepositTransCriteria("123");
        var tableName = "TestTableName";

        // Act
        criteria.CashAccumCreditTbl = tableName;

        // Assert
        Assert.Equal(tableName, criteria.CashAccumCreditTbl);
    }
}