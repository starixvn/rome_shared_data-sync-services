using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Configurations;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace Shared.Jobs
{
    [ExcludeFromCodeCoverage]
    public abstract class BackgroundServiceBase : BackgroundService
    {
        protected readonly ScheduleJob scheduleJob;
        protected readonly ILoggerService loggerService;
        private readonly IMaintenanceService maintenanceService;
        protected readonly int umDelayTimeInSeconds;

        protected BackgroundServiceBase(
            ScheduleJob scheduleJob,
            ILoggerService loggerService,
            IMaintenanceService maintenanceService,
            UMClientConfiguration umClientConfiguration)
        {
            this.scheduleJob = scheduleJob;
            this.loggerService = loggerService;
            this.maintenanceService = maintenanceService;
            umDelayTimeInSeconds = umClientConfiguration.ResultCacheDuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (scheduleJob.IsActive)
            {
                await loggerService.Error(new Exception($"{scheduleJob.JobInfo} is running."), scheduleJob, nameof(ExecuteAsync));

                while (!stoppingToken.IsCancellationRequested)
                {
                    await ExecuteInternal(stoppingToken);
                }
            }
        }

        public abstract Task ExecuteInternal(CancellationToken stoppingToken);

        protected async Task HandleException(Exception exception)
        {
            if (exception.Message.Contains("The account is disabled", StringComparison.OrdinalIgnoreCase))
            {
                loggerService.InfoConsole($"{scheduleJob.JobInfo}: {exception.Message}");
            }
            else
            {
                await loggerService.Error(exception, scheduleJob, nameof(ExecuteInternal));
            }
        }

        protected async Task<UnderMaintenanceTime> GetUnderMaintenanceTime()
        {
            try
            {
                return await maintenanceService.GetUnderMaintenanceTimeAsync();
            }
            catch (Exception ex)
            {
                await loggerService.Error(ex, scheduleJob, nameof(GetUnderMaintenanceTime), "Could not get UM information.");
            }

            return new UnderMaintenanceTime();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (scheduleJob.IsActive)
            {
                await loggerService.Error(new Exception($"{scheduleJob.JobInfo} is stopping."), scheduleJob, nameof(StopAsync));

                await base.StopAsync(cancellationToken);
            }
        }
    }
}