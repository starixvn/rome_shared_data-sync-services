using CashServiceBilling.Criteria;

namespace CashServiceBilling.Tests.Criteria;

public class SyncCashAccumCreditCriteriaTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        const string cashAccumCreditTbl = "TestTable";
        const short custDBGroupId = 123;

        // Act
        var criteria = new SyncCashAccumCreditCriteria(cashAccumCreditTbl, custDBGroupId);

        // Assert
        Assert.Equal(cashAccumCreditTbl, criteria.CashAccumCreditTbl);
        Assert.Equal(custDBGroupId, criteria.CustDBGroupId);
        Assert.Equal(0, criteria.ErrCode);
        Assert.Null(criteria.ErrMsg);
    }

    [Fact]
    public void GetSettingKey_ReturnsExpectedKey()
    {
        // Arrange
        const short custDBGroupId = 456;
        var criteria = new SyncCashAccumCreditCriteria("TestTable", custDBGroupId);

        // Act
        var key = criteria.GetSettingKey();

        // Assert
        Assert.Equal($"CustDB_{custDBGroupId}_Age_CashAccumCredit_SyncData", key);
    }

    [Theory]
    [InlineData("ValidTable", true)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpectedResult(string cashAccumCreditTbl, bool expected)
    {
        // Arrange
        var criteria = new SyncCashAccumCreditCriteria(cashAccumCreditTbl, 123);

        // Act
        var isValid = criteria.IsValid();

        // Assert
        Assert.Equal(expected, isValid);
    }
}