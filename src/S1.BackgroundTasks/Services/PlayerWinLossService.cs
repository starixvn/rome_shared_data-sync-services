using System.Web;

namespace S1.BackgroundTasks.Services;

public interface IPlayerWinLossService
{
    Task<string> GetPlayerWinLoss(DateTimeOffset from, DateTimeOffset to);
}

public class PlayerWinLossService(HttpClient httpClient, Fanex.Logging.ILogger logger) : IPlayerWinLossService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly Fanex.Logging.ILogger logger = logger;

    public async Task<string> GetPlayerWinLoss(DateTimeOffset from, DateTimeOffset to)
    {
        var response = await httpClient.GetStringAsync($"PlayerWinLoss?From={HttpUtility.UrlEncode(from.ToString())}&To={HttpUtility.UrlEncode(to.ToString())}");
        await logger.InfoAsync(response);
        return response;
    }
}