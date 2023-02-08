using BalanceRecord.Criteria;
using Fanex.Data.Repository;

namespace BalanceRecord.Services;

public interface IBalanceRecordService
{
    Task<bool> IsStopJob(string jobName, bool isTest);

    Task<long?> GetLastRowInserted(short siteDBGroupId, bool isTest);

    Task<byte[]> GetDataFromSiteDB(
        string siteDBName,
        long lastRowInserted,
        int batch,
        bool isTest);

    Task<string> InsertDataIntoSiteDB(
        short siteDBGroupId,
        byte[] transInfo,
        bool isTest);
}

public sealed class BalanceRecordService : IBalanceRecordService
{
    private readonly IDynamicRepository dynamicRepository;

    public BalanceRecordService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<bool> IsStopJob(string jobName, bool isTest)
    {
        var command = new GetJobStatusCommand($"BT_{jobName}_ServiceStatus{(isTest ? "_UAT" : string.Empty)}");
        await dynamicRepository.ExecuteAsync(command);

        return "0" == command.VValue;
    }

    public async Task<long?> GetLastRowInserted(short siteDBGroupId, bool isTest)
    {
        var command = new GetLastRowInsertedCommand(siteDBGroupId, isTest);
        await dynamicRepository.ExecuteAsync(command);

        return command.LastRowInserted;
    }

    public async Task<byte[]> GetDataFromSiteDB(string siteDBName, long lastRowInserted, int batch, bool isTest)
    {
        var command = new GetBalanceRecordFromSiteDBCommand(siteDBName, lastRowInserted, batch, isTest);
        await dynamicRepository.ExecuteAsync(command);

        return command.TransInfo;
    }

    public async Task<string> InsertDataIntoSiteDB(
        short siteDBGroupId,
        byte[] transInfo,
        bool isTest)
    {
        var command = new InsertBalanceRecordFromSiteDBCommand(siteDBGroupId, transInfo, isTest);
        await dynamicRepository.ExecuteAsync(command);

        return command.ErrorMsg;
    }
}
