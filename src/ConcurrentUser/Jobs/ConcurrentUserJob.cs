using ConcurrentUser.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

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
        try
        {
            var umInfo = await GetUnderMaintenanceTime();
            if (!umInfo.IsUnderMaintenanceTime)
            {
                await ExecuteJob();
            }

            await Task.Delay(scheduleJob.DelayExecutionTimeInSeconds * 1000, stoppingToken);
        }
        catch (Exception ex)
        {
            await HandleException(ex);
        }
    }

    private async Task ExecuteJob()
    {
        var users = await service.GetConcurrentUser();
        foreach (var server in ServerForInsertData)
        {
            try
            {
                var result = await service.InsertConcurrentUser(users, server);
            }
            catch (Exception exception)
            {
                await loggerService.Error(exception, scheduleJob, "InsertConcurrentUser");
            }
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
