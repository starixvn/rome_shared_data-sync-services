using System.Diagnostics.CodeAnalysis;
using ConcurrentUser.Jobs;
using ConcurrentUser.Services;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using Shared.Configuration;
using Shared.Jobs;
using Shared.Services;

namespace ConcurrentUser.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    private static readonly Dictionary<string, Func<IServiceProvider, ScheduleJob, BackgroundServiceBase>> JobFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ConcurrentUser"] = (sp, job) => new ConcurrentUserJob(
            job,
            sp.GetRequiredService<ILoggerService>(),
            sp.GetRequiredService<IMaintenanceService>(),
            sp.GetRequiredService<UMClientConfiguration>(),
            sp.GetRequiredService<IConcurrentUserService>()),

        ["OnlineUser"] = (sp, job) => new OnlineUserJob(
            job,
            sp.GetRequiredService<ILoggerService>(),
            sp.GetRequiredService<IMaintenanceService>(),
            sp.GetRequiredService<UMClientConfiguration>(),
            sp.GetRequiredService<IOnlineUserService>())
    };

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddSingleton<IConcurrentUserService, ConcurrentUserService>();
        services.AddSingleton<IOnlineUserService, OnlineUserService>();

        var activeJobs = configuration
            .GetSection("ScheduleJobs")
            .Get<ScheduleJob[]>()?
            .Where(job => job.IsActive && JobFactories.ContainsKey(job.Name))
            ?? Array.Empty<ScheduleJob>();

        foreach (var job in activeJobs)
        {
            services.AddSingleton<IHostedService>(sp => JobFactories[job.Name](sp, job));
        }

        return services;
    }
}
