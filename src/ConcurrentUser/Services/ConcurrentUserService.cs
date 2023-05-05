using ConcurrentUser.Criteria;
using ConcurrentUser.Models;
using Shared.Models;
using Fanex.Data.Repository;
using Fanex.Data;

using System.Collections;
namespace ConcurrentUser.Services;

public interface IConcurrentUserService
{
    Task<IEnumerable<ConcurrentUserModel>> GetConcurrentUser();

    Task<Result> InsertConcurrentUser(IEnumerable<ConcurrentUserModel> users, string serverName);

}

public class ConcurrentUserService : IConcurrentUserService
{
    private readonly IDynamicRepository dynamicRepository;

    public ConcurrentUserService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<IEnumerable<ConcurrentUserModel>> GetConcurrentUser()
    {

        var criteria = new GetConcurrentUserCriteria();
        return await dynamicRepository.FetchAsync<ConcurrentUserModel>(criteria);
    }

    public async Task<Result> InsertConcurrentUser(IEnumerable<ConcurrentUserModel> users, string serverName)
    {
        var command = new InsertConcurrentUserCriteria(users, serverName);
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrorMsg);
    }
}
