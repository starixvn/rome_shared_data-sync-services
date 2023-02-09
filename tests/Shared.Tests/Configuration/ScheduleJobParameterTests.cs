using Shared.Configuration;
using Xunit;

namespace Shared.Tests.Configuration;

public class ScheduleJobParameterTests
{
    [Fact]
    public void New_Always_AssignsCorrectProperties()
    {
        // Arrange
        var parameter = new ScheduleJobParameter
        {
            Key = "BatchSize",
            Value = "2000"
        };

        // Act && Assert
        Assert.True("BatchSize" == parameter.Key && "2000" == parameter.Value);
    }
}
