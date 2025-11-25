using ConcurrentUser.Criteria;
using ConcurrentUser.Models;
using Xunit;

namespace ConcurrentUser.Tests.Criteria;

public class InsertOnlineUserCriteriaTests
{
    [Fact]
    public void Constructor_WithUsers_SetsPropertiesCorrectly()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>
        {
            new ConcurrentUserModel
            {
                CreatedDate = new DateTime(2023, 5, 8),
                RealUserCount = 10,
                RoleId = 2,
                SiteId = 100,
                TestingUserCount = 20
            }
        };

        // Act
        var criteria = new InsertOnlineUserCriteria(users);

        // Assert
        Assert.NotNull(criteria.XmlData);
        Assert.Contains("<root>", criteria.XmlData);
        Assert.Contains("</root>", criteria.XmlData);
    }

    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>
        {
            new ConcurrentUserModel
            {
                CreatedDate = new DateTime(2023, 5, 8),
                RealUserCount = 10,
                RoleId = 2,
                SiteId = 100,
                TestingUserCount = 20
            }
        };
        var criteria = new InsertOnlineUserCriteria(users);

        // Act & Assert
        Assert.Equal("Acc_Grafana_OnlineAgentDetail_Ins", criteria.GetSettingKey());
    }

    [Fact]
    public void IsValid_WithUsers_ReturnsTrue()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>
        {
            new ConcurrentUserModel
            {
                CreatedDate = new DateTime(2023, 5, 8),
                RealUserCount = 10,
                RoleId = 2,
                SiteId = 100,
                TestingUserCount = 20
            }
        };
        var criteria = new InsertOnlineUserCriteria(users);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }

    [Fact]
    public void IsValid_WithEmptyUsers_ReturnsFalse()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>();
        var criteria = new InsertOnlineUserCriteria(users);

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Fact]
    public void XmlData_WithMultipleUsers_ContainsAllUsers()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>
        {
            new ConcurrentUserModel
            {
                CreatedDate = new DateTime(2023, 5, 8),
                RealUserCount = 10,
                RoleId = 2,
                SiteId = 100,
                TestingUserCount = 20
            },
            new ConcurrentUserModel
            {
                CreatedDate = new DateTime(2023, 5, 9),
                RealUserCount = 15,
                RoleId = 3,
                SiteId = 101,
                TestingUserCount = 25
            }
        };

        // Act
        var criteria = new InsertOnlineUserCriteria(users);

        // Assert
        Assert.Contains("<root>", criteria.XmlData);
        Assert.Contains("</root>", criteria.XmlData);
        // XML should contain data from both users
        Assert.Contains("100", criteria.XmlData);
        Assert.Contains("101", criteria.XmlData);
    }

    [Fact]
    public void ErrorMsg_CanBeSet()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>
        {
            new ConcurrentUserModel
            {
                CreatedDate = new DateTime(2023, 5, 8),
                RealUserCount = 10,
                RoleId = 2,
                SiteId = 100,
                TestingUserCount = 20
            }
        };
        var criteria = new InsertOnlineUserCriteria(users);

        // Act
        criteria.ErrorMsg = "Test Error";

        // Assert
        Assert.Equal("Test Error", criteria.ErrorMsg);
    }

    [Fact]
    public void ErrCode_CanBeSet()
    {
        // Arrange
        var users = new List<ConcurrentUserModel>
        {
            new ConcurrentUserModel
            {
                CreatedDate = new DateTime(2023, 5, 8),
                RealUserCount = 10,
                RoleId = 2,
                SiteId = 100,
                TestingUserCount = 20
            }
        };
        var criteria = new InsertOnlineUserCriteria(users);

        // Act
        criteria.ErrCode = 1;

        // Assert
        Assert.Equal(1, criteria.ErrCode);
    }
}
