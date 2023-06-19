using ConcurrentUser.Criteria;
using ConcurrentUser.Models;
using NSubstitute;
using Xunit;

namespace ConcurrentUser.Tests.Models;
public class ConcurrentUserModelTests
{
    [Fact]
    public void ToString_Always_ReturnCorrectFormat()
    {
        // Given    
        var user = new ConcurrentUserModel
        {
            CreatedDate = new DateTime(2023, 5, 8),
            RealUserCount = 10,
            RoleId = 2,
            SiteId = 100,
            TestingUserCount = 20
        };
        var expected = "<r CreatedDate=\"5/8/2023 12:00:00 AM\" SiteId=\"100\" RoleId=\"2\" RealUserCount=\"10\" TestingUserCount=\"20\" />";

        // When
        var act = user.ToString();

        // Then
        Assert.Equal(expected, act);
    }
}