using WinlossLimit3rdGame.Criteria;
using Fanex.Data.Repository;
using System.Linq;

namespace WinlossLimit3rdGame.Services;

public interface IWinlossLimitService
{
    Task<bool> InsertWinlossLimit(string data, bool isTest, int settingStatus);

    Task<string> GetWinlossLimit(string dbName);

    Task<bool> ProcessWinlossLimit(bool isTest, int settingStatus);
}

public sealed class WinlossLimitService : IWinlossLimitService
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly ISiteDBConfigService siteDBConfigService;

    public WinlossLimitService(IDynamicRepository dynamicRepository, ISiteDBConfigService siteDBConfigService)
        => (this.dynamicRepository, this.siteDBConfigService) = (dynamicRepository, siteDBConfigService);

    public async Task<bool> ProcessWinlossLimit(bool isTest, int settingStatus)
    {
        var siteDBs = await siteDBConfigService.GetAll();
        var task = new List<Task>();

        foreach (var sitedb in siteDBs)
        {
             task.Add(Process(sitedb.LinkedServerName, isTest, settingStatus));
        }
        
        await Task.WhenAll(task);

        return true;
    }

    public async Task<bool> InsertWinlossLimit(string data, bool isTest, int settingStatus)
    {
        var command = new InsertWinlossLimitCommand(settingStatus, data, isTest);
        await dynamicRepository.ExecuteAsync(command);

        return true;
    }

    public async Task<string> GetWinlossLimit(string dbName)
    {
        var command = new GetWinlossLimitCommand(dbName);
        await dynamicRepository.ExecuteAsync(command);
        return command.Balance;
    }


    private async Task<bool> Process(string dbName, bool isTest, int settingStatus)
    {
        var data = await GetWinlossLimit(dbName);
        await InsertWinlossLimit(data, isTest, settingStatus);

        return true;
    }
}
