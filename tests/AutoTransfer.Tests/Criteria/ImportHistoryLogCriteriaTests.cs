using AutoTransfer.Criteria;

namespace AutoTransfer.Tests.Criteria;

public class ImportHistoryLogCriteriaTests
{
    [Theory]
    [InlineData(0, "Age_ClearBalanceCust_Job_ProcessHistoryImport")]
    [InlineData(1, "Age_ClearBalanceCust_Job_ProcessHistoryImport_HA1")]
    public void GetSettingKey_Always_ReturnsValidSettingKey(int replicateNo, string expectedValue)
    {
        // Arrange
        var criteria = new ImportHistoryLogCriteria("", replicateNo);

        // Act
        var actualValue = criteria.GetSettingKey();

        //Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    [InlineData("log", true)]
    public void IsValid_Always_ReturnsCorrectValue(string rowData, bool expectedValue)
    {
        // Arrange
        var criteria = new ImportHistoryLogCriteria(rowData, 0);

        // Act
        var actualValue = criteria.IsValid();

        //Assert
        Assert.Equal(expectedValue, actualValue);
    }
}
