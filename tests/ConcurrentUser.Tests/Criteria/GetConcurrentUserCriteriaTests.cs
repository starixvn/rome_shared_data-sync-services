using ConcurrentUser.Criteria;
using NSubstitute;
using Xunit;

namespace ConcurrentUser.Tests.Criteria;

public class GetConcurrentUserCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetConcurrentUserCriteria();

        // Act & Assert
        Assert.Equal("Age_Rpt_ConcurrentOnlineUserDetail_Sel", criteria.GetSettingKey());
    }

    [Fact]
    public void IsValid_Always_ReturnsTrue()
    {
        // Arrange
        var criteria = new GetConcurrentUserCriteria();

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}
