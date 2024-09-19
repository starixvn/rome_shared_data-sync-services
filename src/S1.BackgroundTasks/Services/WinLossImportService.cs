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

    public async Task ImportPlayerWinLoss(DateTime fromDate, DateTime toDate, string playerWinLoss)
    {
        var command = new ImportWinLossDataCommand(fromDate, toDate, playerWinLoss);
        await dynamicRepository.ExecuteAsync(command);
        if (command.ErrCode > 0)
        {
            await logger.ErrorAsync(command.ErrMessage);
        }
    }
}
