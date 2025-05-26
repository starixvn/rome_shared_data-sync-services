using System.Diagnostics.CodeAnalysis;
using Shared.Configuration;
using ResetPotentialWinLimit.Jobs;
using ResetPotentialWinLimit.Services;

namespace ResetPotentialWinLimit.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var isTest = configuration.GetValue<bool>("IsTest");
        var scheduleJobs = configuration.GetSection("ScheduleJobs")
            .Get<ScheduleJob[]>()?
            .Where(job => job.IsActive)
            .Select(job =>
            {
                job.IsTest = isTest;
                return job;
            })
            .ToArray() ?? Array.Empty<ScheduleJob>();

        return services
            .AddSingleton(scheduleJobs)
            .AddSingleton<IResetPotentialWinLimitService, ResetPotentialWinLimitService>()
            .AddHostedService<ResetPotentialWinLimitJob>();
    }
}
