using Xunit;
using ResetPotentialWinLimit.Criteria;

namespace ResetPotentialWinLimit.Tests.Criteria;

public class UpdateJobStatusCommandTests
{
    [Fact]
    public void Constructor_WhenValidParametersProvided_ShouldInitializeProperties()
    {
        // Arrange
        string vname = "TestName";
        string vvalue = "TestValue";

        // Act
        var command = new UpdateJobStatusCommand(vname, vvalue);

        // Assert
        Assert.Equal(vname, command.VName);
        Assert.Equal(vvalue, command.VValue);
    }

    [Fact]
    public void GetSettingKey_WhenCalled_ShouldReturnCorrectKey()
    {
        // Arrange
        var command = new UpdateJobStatusCommand("TestName", "TestValue");

        // Act
        var result = command.GetSettingKey();

        // Assert
        Assert.Equal("SystemSettings_Update", result);
    }

    [Theory]
    [InlineData("ValidName", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_WhenVNameProvided_ShouldReturnExpectedResult(string vname, bool expectedResult)
    {
        // Arrange
        var command = new UpdateJobStatusCommand(vname, "TestValue");

        // Act
        var result = command.IsValid();

        // Assert
        Assert.Equal(expectedResult, result);
    }
} 