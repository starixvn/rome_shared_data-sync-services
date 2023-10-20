using AutoTransfer.Criteria;

namespace AutoTransfer.Tests.Criteria;

public class ImportMonitorLogCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new ImportMonitorLogCriteria(string.Empty);

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal("Age_ClearBalanceCust_Job_ProcessExport", actualValue);
    }

    [Fact]
    public void IsValid_Always_ReturnsTrue()
    {
        // Arrange
        var criteria = new ImportMonitorLogCriteria(string.Empty);

        // Act
        var actualValue = criteria.IsValid();

        //Assert
        Assert.True(actualValue);
    }
}
