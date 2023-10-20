using AutoTransfer.Criteria;

namespace AutoTransfer.Tests.Criteria;

public class ImportHistoryLogCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new ImportHistoryLogCriteria("");

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal("Age_ClearBalanceCust_Job_ProcessHistoryImport", actualValue);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    [InlineData("log", true)]
    public void IsValid_Always_ReturnsCorrectValue(string rowData, bool expectedValue)
    {
        // Arrange
        var criteria = new ImportHistoryLogCriteria(rowData);

        // Act
        var actualValue = criteria.IsValid();

        //Assert
        Assert.Equal(expectedValue, actualValue);
    }
}
