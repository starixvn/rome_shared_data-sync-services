using System.Web;

namespace S1.BackgroundTasks.Services;

public interface IPlayerWinLossService
{
    Task<string> GetPlayerWinLoss(DateTimeOffset from, DateTimeOffset to);
}

public class PlayerWinLossService(HttpClient httpClient) : IPlayerWinLossService
{
    private readonly HttpClient httpClient = httpClient;

    public Task<string> GetPlayerWinLoss(DateTimeOffset from, DateTimeOffset to)
        => httpClient.GetStringAsync($"PlayerWinLoss?From={HttpUtility.UrlEncode(from.ToString())}&To={HttpUtility.UrlEncode(to.ToString())}");
}