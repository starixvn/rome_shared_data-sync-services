using ConcurrentUser.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;
using System.Linq;
using System.Collections;
using System.Diagnostics;

namespace ConcurrentUser.Jobs;

public sealed class ConcurrentUserJob : BackgroundServiceBase
{
    private readonly IConcurrentUserService service;
    private List<string> ServerForInsertData = new List<string>();

    public ConcurrentUserJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IConcurrentUserService service)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
        this.service = service;
        AssignParameters(scheduleJob.Parameters);
    }

    public override async Task ExecuteInternal(CancellationToken stoppingToken)
    {
        var umInfo = await GetUnderMaintenanceTime();
        if (!umInfo.IsUnderMaintenanceTime)
        {
            await ExecuteJob();
        }

        await Task.Delay(scheduleJob.DelayExecutionTimeInSeconds * 1000, stoppingToken);
    }

    private async Task ExecuteJob()
    {
        try
        {
            var users = await service.GetConcurrentUser();
            foreach (var server in ServerForInsertData)
            {
                try
                {
                    var result = await service.InsertConcurrentUser(users, server);
                    loggerService.InfoConsole($"Inserted Concurrent users to server({server}) has been {(result.IsOK ? "OK" : "Error!")}");
                }
                catch (Exception exception)
                {
                    loggerService.Error(exception, scheduleJob, "InsertConcurrentUser");
                }
            }

            loggerService.InfoConsole(
                $"{scheduleJob.Name} has sleep {scheduleJob.DelayExecutionTimeInSeconds} seconds");
        }
        catch (Exception ex)
        {
            await HandleException(ex);
        }
    }

    private void AssignParameters(ScheduleJobParameter[] parameters)
    {
        foreach (var item in parameters)
        {
            ServerForInsertData.Add(item.Value);
        }
    }
}
