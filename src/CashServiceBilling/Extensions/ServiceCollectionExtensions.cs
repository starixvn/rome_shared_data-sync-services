using System.Diagnostics.CodeAnalysis;
using Shared.Configuration;
using CashServiceBilling.Jobs;
using CashServiceBilling.Services;
using CashServiceBilling.Models;

namespace CashServiceBilling.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JobsSettingsModel>(options => configuration.GetSection("JobsSettings").Bind(options));
        var isTest = configuration.GetSection("JobsSettings:IsTest").Get<bool>();
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
            .AddSingleton<ICashServiceBillingService, CashServiceBillingService>()
            .AddHostedService<CashServiceBillingJob>()
            .AddSingleton<IAccumulatedCreditService, AccumulatedCreditService>()
            .AddSingleton<AccumulatedCreditJob>();
    }
}
