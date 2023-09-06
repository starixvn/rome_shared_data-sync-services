using AutoTransfer.Models;
using AutoTransfer.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace AutoTransfer.Jobs
{
    public sealed class AutoTransferNotificationsJob : BackgroundServiceBase
    {
        public readonly JobsSettingsModel settings;
        public readonly ITransferNotificationsService notificationsService;
        private const int UMDelayCheckingTimeInMilliseconds = 60000;

        public AutoTransferNotificationsJob(
            ScheduleJob[] scheduleJobs,
            ILoggerService loggerService,
            IMaintenanceService maintenanceService,
            UMClientConfiguration umClientConfiguration,
            ITransferNotificationsService notificationsService,
            IOptions<JobsSettingsModel> settings) : base(
                scheduleJobs.FirstOrDefault(job => job.Name.Equals(nameof(AutoTransferNotificationsJob), StringComparison.OrdinalIgnoreCase)) ?? new ScheduleJob(),
                loggerService,
                maintenanceService,
                umClientConfiguration)
        {
            this.settings = settings.Value;
            this.notificationsService = notificationsService;
        }

        public override async Task ExecuteInternal(CancellationToken stoppingToken)
        {
            try
            {
                var umInfo = await GetUnderMaintenanceTime();
                if (umInfo.IsUnderMaintenanceTime)
                {
                    await Task.Delay(settings.StartJobCheckingIntervalInMilliSeconds, stoppingToken);
                    return;
                }

                foreach (var siteDB in settings.SiteDBs)
                {
                    await notificationsService.InsertNotifications(siteDB);
                }
            }
            catch (Exception e)
            {
                await HandleException(e);
            }

            await Task.Delay(settings.StartJobCheckingIntervalInMilliSeconds, stoppingToken);
        }
    }
}