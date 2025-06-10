using Xunit;
using ResetPotentialWinLimit.Criteria;

namespace ResetPotentialWinLimit.Tests.Criteria;

public class GetJobStatusCommandTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnsSystemSettingsSelect()
    {
        // Arrange
        var command = new GetJobStatusCommand("testVName");

        // Act
        var result = command.GetSettingKey();

        // Assert
        Assert.Equal("SystemSettings_Select", result);
    }

    [Theory]
    [InlineData("testVName", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_WhenVNameProvided_ReturnsExpectedResult(string vname, bool expectedResult)
    {
        // Arrange
        var command = new GetJobStatusCommand(vname);

        // Act
        var result = command.IsValid();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void Constructor_WhenVNameProvided_SetsVNameProperty()
    {
        // Arrange
        var expectedVName = "testVName";

        // Act
        var command = new GetJobStatusCommand(expectedVName);

        // Assert
        Assert.Equal(expectedVName, command.VName);
    }

    [Fact]
    public void VValue_WhenSet_CanBeRetrieved()
    {
        // Arrange
        var command = new GetJobStatusCommand("testVName");
        var expectedValue = "testValue";

        // Act
        command.VValue = expectedValue;

        // Assert
        Assert.Equal(expectedValue, command.VValue);
    }
} 