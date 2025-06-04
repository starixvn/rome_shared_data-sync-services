using Xunit;
using ResetPotentialWinLimit.Criteria;

namespace ResetPotentialWinLimit.Tests.Criteria;

public class SelectMemberListCriteriaTests
{
    [Fact]
    public void Constructor_WhenValidParametersProvided_ShouldInitializeProperties()
    {
        // Arrange
        var batchSize = 100;
        var lastMaxCustID = 500;

        // Act
        var criteria = new SelectMemberListCriteria(batchSize, lastMaxCustID);

        // Assert
        Assert.Equal(batchSize, criteria.BatchSize);
        Assert.Equal(lastMaxCustID, criteria.LastMaxCustID);
    }

    [Fact]
    public void GetSettingKey_WhenCalled_ShouldReturnCorrectKey()
    {
        // Arrange
        var criteria = new SelectMemberListCriteria(100, 500);

        // Act
        var settingKey = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Task_ResetAccumWinLoss_MemberListSel", settingKey);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValid_WhenBatchSizeProvided_ShouldReturnExpectedResult(int batchSize, bool expectedResult)
    {
        // Arrange
        var criteria = new SelectMemberListCriteria(batchSize, 500);

        // Act
        var isValid = criteria.IsValid();

        // Assert
        Assert.Equal(expectedResult, isValid);
    }
} 