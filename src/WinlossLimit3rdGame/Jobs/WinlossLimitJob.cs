using WinlossLimit3rdGame.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace WinlossLimit3rdGame.Jobs;

public sealed class WinlossLimitJob : IntervalBackgroundService
{
    private readonly IWinlossLimitService winlossLimitService;

    public WinlossLimitJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IWinlossLimitService winlossLimitService)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration)
    {
        this.winlossLimitService = winlossLimitService;
    }

    public override async Task<bool> ExecuteJob()
    => await winlossLimitService.ProcessWinlossLimit(scheduleJob.IsTest, scheduleJob.SettingStatus);
}
