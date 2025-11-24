using System.Diagnostics.CodeAnalysis;
using ConcurrentUser.Jobs;
using ConcurrentUser.Services;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace ConcurrentUser.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddSingleton<IConcurrentUserService, ConcurrentUserService>();
        services.AddSingleton<IOnlineUserService, OnlineUserService>();

        var activeJobs = configuration
            .GetSection("ScheduleJobs")
            .Get<ScheduleJob[]>()?
            .Where(job => job.IsActive) ?? Array.Empty<ScheduleJob>();

        foreach (var job in activeJobs)
        {
            services.AddSingleton<IHostedService>(sp => CreateHostedService(sp, job));
        }

        return services;
    }

    private static BackgroundServiceBase CreateHostedService(IServiceProvider sp, ScheduleJob job)
    {
        var loggerService = sp.GetRequiredService<ILoggerService>();
        var maintenanceService = sp.GetRequiredService<IMaintenanceService>();
        var umConfig = sp.GetRequiredService<UMClientConfiguration>();

        return job.Name.ToLowerInvariant() switch
        {
            var name when name.Contains("concurrent user") => new ConcurrentUserJob(
                job, loggerService, maintenanceService, umConfig,
                sp.GetRequiredService<IConcurrentUserService>()),

            var name when name.Contains("online user") => new OnlineUserJob(
                job, loggerService, maintenanceService, umConfig,
                sp.GetRequiredService<IOnlineUserService>()),

            _ => throw new InvalidOperationException($"Unknown job type: {job.Name}")
        };
    }
}
