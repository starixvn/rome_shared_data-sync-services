using BalanceRecord.Criteria;
using Xunit;

namespace BalanceRecord.Tests.Criteria;

public class GetBalanceRecordFromSiteDBCommandTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetBalanceRecordFromSiteDBCommand("SiteDB01", 1, 2000, false);

        // Act & Assert
        Assert.Equal("SiteDB_BalanceTrigger_Select_SiteDB01", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, true)]
    [InlineData(100, true)]
    public void IsValid_LastRowInserted_ReturnsCorrectExpected(long lastRowInserted, bool expected)
    {
        // Arrange
        var criteria = new GetBalanceRecordFromSiteDBCommand("SiteDB01", lastRowInserted, 2000, false);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_Batch_ReturnsCorrectExpected(int batch, bool expected)
    {
        // Arrange
        var criteria = new GetBalanceRecordFromSiteDBCommand("SiteDB01", 1, batch, false);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange
        var criteria = new GetBalanceRecordFromSiteDBCommand("SiteDB01", 1, 2000, true)
        {
            TransInfo = new byte[] { 1, 2, 3, 4, 5, 6 }
        };

        // Act && Assert
        Assert.True(
            "SiteDB01" == criteria.SiteDBName
            && criteria.IsTest
            && 1 == criteria.TransInfo[0]
            && 2 == criteria.TransInfo[1]
            && 3 == criteria.TransInfo[2]
            && 4 == criteria.TransInfo[3]
            && 5 == criteria.TransInfo[4]
            && 6 == criteria.TransInfo[5]
        );
    }
}
