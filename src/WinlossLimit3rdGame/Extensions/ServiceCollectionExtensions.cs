using System.Diagnostics.CodeAnalysis;
using WinlossLimit3rdGame.Jobs;
using WinlossLimit3rdGame.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace WinlossLimit3rdGame.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IWinlossLimitService, WinlossLimitService>();       

        var isTest = configuration.GetValue<bool>("IsTest");
        var settingStatus = configuration.GetValue<int>("SettingStatus");
        configuration
            .GetSection("ScheduleJobs")
            .Get<ScheduleJob[]>()?
            .Where(job => job.IsActive)
            .ToList()
            .ForEach(job =>
            {
                job.IsTest = isTest;
                job.SettingStatus = settingStatus;
                services.AddSingleton<IHostedService>(sp => new WinlossLimitJob(
                    job,
                    sp.GetRequiredService<ILoggerService>(),
                    sp.GetRequiredService<IMaintenanceService>(),
                    sp.GetRequiredService<UMClientConfiguration>(),
                    sp.GetRequiredService<IWinlossLimitService>()));
            });

        return services;
    }
}
