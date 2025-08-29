
using Shared.Configuration;

namespace Shared.Services;

public interface ILoggerService
{
    Task Info(string message);

    Task Error(string message);

    Task Error(Exception exception, ScheduleJob scheduleJob, string methodName, string message = "");

    void InfoConsole(string message, params object[] args);

    Task Notify(string message, string category);
}