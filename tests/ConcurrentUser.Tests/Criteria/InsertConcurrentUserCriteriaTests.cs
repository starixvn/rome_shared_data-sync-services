using ConcurrentUser.Criteria;
using ConcurrentUser.Models;

using Xunit;

namespace ConcurrentUser.Tests.Criteria;

public class InsertConcurrentUserCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsSettingKeyAndServername()
    {
        // Arrange
        var criteria = new InsertConcurrentUserCriteria(new List<ConcurrentUserModel>(), "DB05");

        // Act & Assert
        Assert.Equal("Age_Rpt_ConcurrentOnlineUserDetail_Ins_DB05", criteria.GetSettingKey());
    }

    [Fact]
    public void IsValid_ServerNameIsEmpty_ReturnsFalse()
    {
        // Arrange
        var criteria = new InsertConcurrentUserCriteria(new List<ConcurrentUserModel>(), string.Empty);

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Fact]
    public void IsValid_UsersAreEmpty_ReturnsFalse()
    {
        // Arrange
        var criteria = new InsertConcurrentUserCriteria(new List<ConcurrentUserModel>(), "DB05");

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Fact]
    public void XmlData_Always_ReturnsCorrectFormat()
    {
        // Arrange
        var user = new ConcurrentUserModel
        {
            CreatedDate = new DateTime(2023, 5, 8),
            RealUserCount = 10,
            RoleId = 2,
            SiteId = 100,
            TestingUserCount = 20
        };
        var criteria = new InsertConcurrentUserCriteria(new List<ConcurrentUserModel> { user }, "DB05");
        var expected = "<root><r CreatedDate=\"5/8/2023 12:00:00 AM\" SiteId=\"100\" RoleId=\"2\" RealUserCount=\"10\" TestingUserCount=\"20\" /></root>";

        // Act && Assert
        Assert.Equal(expected, criteria.XmlData);
    }
}
