using System.Diagnostics.CodeAnalysis;
using BalanceRecord.Jobs;
using BalanceRecord.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace BalanceRecord.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBalanceRecordService, BalanceRecordService>();

        var isTest = configuration.GetValue<bool>("IsTest");
        configuration.GetSection("ScheduleJobs")
            .Get<ScheduleJob[]>()
            .Where(job => job.IsActive)
            .ToList()
            .ForEach(job =>
            {
                job.IsTest = isTest;
                services.AddSingleton<IHostedService>(sp => new BalanceRecordJob(
                    job,
                    sp.GetRequiredService<ILoggerService>(),
                    sp.GetRequiredService<IMaintenanceService>(),
                    sp.GetRequiredService<UMClientConfiguration>(),
                    sp.GetRequiredService<IBalanceRecordService>()));
            });

        return services;
    }
}
