using System.Diagnostics.CodeAnalysis;
using ConcurrentUser.Jobs;
using ConcurrentUser.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Services;

namespace ConcurrentUser.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConcurrentUserService, ConcurrentUserService>();
        configuration
            .GetSection("ScheduleJobs")
            .Get<ScheduleJob[]>()?
            .Where(job => job.IsActive)
            .ToList()
            .ForEach(job =>
            {
                services.AddSingleton<IHostedService>(sp => new ConcurrentUserJob(
                    job,
                    sp.GetRequiredService<ILoggerService>(),
                    sp.GetRequiredService<IMaintenanceService>(),
                    sp.GetRequiredService<UMClientConfiguration>(),
                    sp.GetRequiredService<IConcurrentUserService>()));
            });

        return services;
    }
}
