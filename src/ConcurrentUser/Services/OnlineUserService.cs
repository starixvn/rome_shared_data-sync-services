using ConcurrentUser.Criteria;
using ConcurrentUser.Models;
using Shared.Models;
using Fanex.Data.Repository;
using Fanex.Data;

using System.Collections;
namespace ConcurrentUser.Services;

public interface IOnlineUserService
{
    Task<IEnumerable<ConcurrentUserModel>> GetOnlineUser();

    Task<Result> InsertOnlineUser(IEnumerable<ConcurrentUserModel> users);

}

public class OnlineUserService : IOnlineUserService
{
    private readonly IDynamicRepository dynamicRepository;

    public OnlineUserService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<IEnumerable<ConcurrentUserModel>> GetOnlineUser()
    {
        var criteria = new GetConcurrentUserCriteria();
        return await dynamicRepository.FetchAsync<ConcurrentUserModel>(criteria);
    }

    public async Task<Result> InsertOnlineUser(IEnumerable<ConcurrentUserModel> users)
    {
        var command = new InsertConcurrentUserCriteria(users, "");
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode.ToString(), command.ErrorMsg);
    }
}
