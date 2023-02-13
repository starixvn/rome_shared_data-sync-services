using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;
using Fanex.Data;
using Microsoft.Extensions.Logging;
using Shared.Configuration;

namespace Shared.Services;

public interface ILoggerService
{
    Task Info(string message);

    Task Error(Exception exception, ScheduleJob scheduleJob, string methodName, string message = "");

    void InfoConsole(string message, params object[] args);
}

[ExcludeFromCodeCoverage]
public sealed class LoggerService : ILoggerService
{
    private readonly Fanex.Logging.ILogger logger;
    private readonly ILogger<LoggerService> loggerConsole;

    public LoggerService(Fanex.Logging.ILogger logger, ILogger<LoggerService> loggerConsole)
        => (this.logger, this.loggerConsole) = (logger, loggerConsole);

    public Task Info(string message) => logger.InfoAsync(message);

    public Task Error(Exception exception, ScheduleJob scheduleJob, string methodName, string message = "")
        => logger.ErrorAsync(BuildError(exception, scheduleJob, methodName, message));

    private static string BuildError(Exception exception, ScheduleJob scheduleJob, string methodName, string message)
    {
        var errorBuilder = new StringBuilder();
        errorBuilder
            .AppendLine(exception.Message)
            .AppendLine();

        BuildError(errorBuilder, scheduleJob, methodName, message);

        if (exception is ObjectDbException objectDbException
            && exception.InnerException is SqlException sqlException)
        {
            BuildDbException(errorBuilder, objectDbException, sqlException);
        }

        errorBuilder.Append("EXCEPTION: ").Append(exception.ToString());

        return errorBuilder.ToString();
    }

    private static void BuildError(
        StringBuilder errorBuilder,
        ScheduleJob scheduleJob,
        string methodName,
        string message)
        => errorBuilder
            .Append(scheduleJob.JobInfo).Append(": ").AppendLine(methodName)
            .Append("Error: ").AppendLine(message)
            .AppendLine();

    private static void BuildDbException(
        StringBuilder errorBuilder,
        ObjectDbException dbException,
        SqlException sqlException)
    {
        errorBuilder
            .AppendLine("DATABASE INFO:")
            .Append("Server: ").AppendLine(sqlException.Server)
            .Append("DbName: ").AppendLine(dbException.DbName)
            .Append("SpName: ").AppendLine(dbException.SpName)
            .AppendLine("Parameters: ");

        foreach (var item in dbException.Parameters)
        {
            errorBuilder
                .Append(item.Key)
                .Append('=')
                .AppendLine(HttpUtility.HtmlEncode(item.Value.ToString()));
        }

        errorBuilder
            .Append("LineNumber: ").AppendLine(sqlException.LineNumber.ToString())
            .AppendLine();
    }

    public void InfoConsole(string message, params object[] args)
        => loggerConsole.LogInformation(message, args);
}
