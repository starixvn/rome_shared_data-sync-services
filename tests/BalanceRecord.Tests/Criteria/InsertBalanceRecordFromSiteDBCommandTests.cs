using BalanceRecord.Criteria;
using Xunit;

namespace BalanceRecord.Tests.Criteria;

public class InsertBalanceRecordFromSiteDBCommandTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new InsertBalanceRecordFromSiteDBCommand(1, new byte[] { 1, 2, 3 }, false);

        // Act & Assert
        Assert.Equal("BalanceTriggerSiteDB_Insert", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_Always_ReturnsCorrectExpected(short siteDBGroupId, bool expected)
    {
        // Arrange
        var criteria = new InsertBalanceRecordFromSiteDBCommand(siteDBGroupId, new byte[] { 1, 2, 3 }, false);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange
        var criteria = new InsertBalanceRecordFromSiteDBCommand(1, new byte[] { 1, 2, 3 }, true)
        {
            ErrorMsg = "Invalid object odb"
        };

        // Act && Assert
        Assert.True(
            1 == criteria.TransInfo[0]
            && 2 == criteria.TransInfo[1]
            && 3 == criteria.TransInfo[2]
            && criteria.IsTest
            && "Invalid object odb" == criteria.ErrorMsg
        );
    }
}
