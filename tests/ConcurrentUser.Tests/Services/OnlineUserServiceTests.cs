using ConcurrentUser.Criteria;
using ConcurrentUser.Models;
using ConcurrentUser.Services;
using Shared.Models;
using Fanex.Data.Repository;
using NSubstitute;
using Xunit;

namespace ConcurrentUser.Tests.Services;

public class OnlineUserServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly IOnlineUserService onlineUserService;

    public OnlineUserServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        onlineUserService = new OnlineUserService(dynamicRepository);
    }

    [Fact]
    public async Task GetOnlineUser_Always_CallsFetchAsync()
    {
        // Arrange

        // Act
        await onlineUserService.GetOnlineUser();

        // Assert
        await dynamicRepository.Received().FetchAsync<ConcurrentUserModel>(Arg.Any<GetConcurrentUserCriteria>());
    }

    [Fact]
    public async Task InsertOnlineUser_Always_CallsExecuteAsync()
    {
        // Arrange

        // Act
        await onlineUserService.InsertOnlineUser(new List<ConcurrentUserModel>());

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Any<InsertOnlineUserCriteria>());
    }

    [Fact]
    public async Task InsertOnlineUser_Always_ReturnResult()
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

        dynamicRepository
        .When(x => x.ExecuteAsync(Arg.Any<InsertOnlineUserCriteria>()))
        .Do(x =>
        {
            var criteria = (InsertOnlineUserCriteria)x[0];
            if (criteria != null)
            {
                criteria.ErrCode = 0;
                criteria.ErrorMsg = "Success";
            }
        });

        // Act
        var act = await onlineUserService.InsertOnlineUser(new List<ConcurrentUserModel> { user });

        // Assert
        Assert.Equal("0", act.ErrorCode);
        Assert.Equal("Success", act.ErrorMessage);
        Assert.True(act.IsOK);
    }

    [Fact]
    public async Task GetOnlineUser_Always_ReturnsUsers()
    {
        // Arrange
        var expectedUsers = new List<ConcurrentUserModel>
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

        dynamicRepository
            .FetchAsync<ConcurrentUserModel>(Arg.Any<GetConcurrentUserCriteria>())
            .Returns(expectedUsers);

        // Act
        var result = await onlineUserService.GetOnlineUser();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUsers.Count, result.Count());
    }
}
