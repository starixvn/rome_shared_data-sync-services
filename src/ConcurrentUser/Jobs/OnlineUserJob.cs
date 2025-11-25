using ConcurrentUser.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace ConcurrentUser.Jobs;

public sealed class OnlineUserJob : BackgroundServiceBase
{
    private readonly IOnlineUserService service;

    public OnlineUserJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IOnlineUserService service)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
        this.service = service;
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
        var users = await service.GetOnlineUser();
        try
        {
            await service.InsertOnlineUser(users);
        }
        catch (Exception exception)
        {
            await loggerService.Error(exception, scheduleJob, "InsertOnlineUser");
        }
    }
}
