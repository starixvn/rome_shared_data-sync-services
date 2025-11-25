using ConcurrentUser.Criteria;
using ConcurrentUser.Models;
using ConcurrentUser.Services;
using Shared.Models;
using Fanex.Data.Repository;
using NSubstitute;
using Xunit;

namespace ConcurrentUser.Tests.Services;

public class ConcurrentUserServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly IConcurrentUserService concurrentUserService;

    public ConcurrentUserServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        concurrentUserService = new ConcurrentUserService(dynamicRepository);
    }

    [Fact]
    public async Task GetConcurrentUser_Always_CallsFetchAsync()
    {
        // Arrange

        // Act
        await concurrentUserService.GetConcurrentUser();

        // Assert
        await dynamicRepository.Received().FetchAsync<ConcurrentUserModel>(Arg.Any<GetConcurrentUserCriteria>());
    }

    [Fact]
    public async Task InsertConcurrentUser_Always_CallsExecuteAsync()
    {
        // Arrange

        // Act
        await concurrentUserService.InsertConcurrentUser(new List<ConcurrentUserModel>(), "DB05");

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Any<InsertConcurrentUserCriteria>());
    }

    [Fact]
    public async Task InsertConcurrentUser_Always_ReturnResult()
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
        .When(x => x.ExecuteAsync(Arg.Any<InsertConcurrentUserCriteria>()))
        .Do(x =>
        {
            var criteria = (InsertConcurrentUserCriteria)x[0];
            if (criteria != null)
            {
                criteria.ErrCode =0;
                criteria.ErrorMsg = "Su";
            }
        });

        // Act
        var act = await concurrentUserService.InsertConcurrentUser(new List<ConcurrentUserModel>(), "DB05");

        // Assert
        Assert.Equal("0", act.ErrorCode);
        Assert.Equal("Su", act.ErrorMessage);
        Assert.True(act.IsOK);
    }
}
