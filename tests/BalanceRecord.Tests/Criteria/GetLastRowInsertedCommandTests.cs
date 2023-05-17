using BalanceRecord.Criteria;
using Xunit;

namespace BalanceRecord.Tests.Criteria;

public class GetLastRowInsertedCommandTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetLastRowInsertedCommand(1, false);

        // Act & Assert
        Assert.Equal("BalanceTriggerSiteDBConfig_Select", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_Always_ReturnsCorrectExpected(short siteDBGroupId, bool expected)
    {
        // Arrange
        var criteria = new GetLastRowInsertedCommand(siteDBGroupId, false);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange
        var criteria = new GetLastRowInsertedCommand(1, true)
        {
            LastRowInserted = 1001
        };

        // Act && Assert
        Assert.True(criteria.IsTest && 1001 == criteria.LastRowInserted);
    }
}
