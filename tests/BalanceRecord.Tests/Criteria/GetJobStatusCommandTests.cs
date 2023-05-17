using BalanceRecord.Criteria;
using Xunit;

namespace BalanceRecord.Tests.Criteria;

public class GetJobStatusCommandTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetJobStatusCommand("SiteDB01");

        // Act & Assert
        Assert.Equal("SystemSettings_Select", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("      ", false)]
    [InlineData("SiteDB01", true)]
    public void IsValid_Always_ReturnsCorrectExpected(string vname, bool expected)
    {
        // Arrange
        var criteria = new GetJobStatusCommand(vname);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange
        var criteria = new GetJobStatusCommand("SiteDB01")
        {
            VValue = "1"
        };

        // Act & Assert
        Assert.Equal("1", criteria.VValue);
    }
}
