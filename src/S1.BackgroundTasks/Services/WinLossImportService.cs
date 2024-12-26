using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Fanex.Data.Repository;
using S1.BackgroundTasks.Criteria;

namespace S1.BackgroundTasks.Services;

public record Schedule
{
    public DateTime FromDate { get; init; }

    public DateTime ToDate { get; init; }
}

public interface IWinLossImportService
{
    public Task ImportPlayerWinLoss(DateTime fromDate, DateTime toDate, string playerWinLoss);

    public Task<IEnumerable<Schedule>> GetSchedules();
}


public class WinLossImportService(IDynamicRepository dynamicRepository, Fanex.Logging.ILogger logger) : IWinLossImportService
{
    private readonly IDynamicRepository dynamicRepository = dynamicRepository;
    private readonly Fanex.Logging.ILogger logger = logger;

    public Task<IEnumerable<Schedule>> GetSchedules()
        => dynamicRepository.FetchAsync<Schedule>(new GetSchedulesCriteria());

    public Task ImportPlayerWinLoss(DateTime fromDate, DateTime toDate, string playerWinLoss)
    {
        var insertDB052Task = ImportPlayerWinLoss(new DB052ImportWinLossDataCommand(fromDate, toDate, playerWinLoss));
        var insertDB053Task = ImportPlayerWinLoss(new DB053ImportWinLossDataCommand(fromDate, toDate, playerWinLoss));

        return Task.WhenAll(insertDB052Task, insertDB053Task);
    }

    private async Task ImportPlayerWinLoss(ImportWinLossDataBaseCommand command)
    {
        await dynamicRepository.ExecuteAsync(command);
        if (command.ErrCode > 0)
        {
            await logger.ErrorAsync(command.ErrMessage);
        }
    }
}
