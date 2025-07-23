using System.Diagnostics.CodeAnalysis;
using Shared.Configuration;
using CashServiceBilling.Jobs;
using CashServiceBilling.Services;
using CashServiceBilling.Models;
using Microsoft.AspNetCore.Builder;
using Hangfire;

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
            .AddSingleton<ITransactionLogService, TransactionLogService>()
            .AddSingleton<IAccumulatedCreditService, AccumulatedCreditService>()
            .AddSingleton<AccumulatedCreditJob>()
            .AddSingleton<ITransactionService, TransactionService>()
            .AddHostedService<UpdateTransExpiredStatusJob>()
            .AddSingleton<ICalculateBonusService, CalculateBonusService>()
            .AddSingleton<CalculateBonusJob>();
    }

    public static IApplicationBuilder AddJobs(this IApplicationBuilder app)
    {
        var jobOptions = new RecurringJobOptions { TimeZone = TimeZoneInfo.Local };

        var accumulatedCreditJob = app.ApplicationServices.GetRequiredService<AccumulatedCreditJob>();
        RecurringJob.AddOrUpdate(
            "Returning-Accumulated-Credit-To-Agent",
            () => accumulatedCreditJob.ExecuteAsync(),
            accumulatedCreditJob.Cron,
            jobOptions);

        var calculateBonusJob = app.ApplicationServices.GetRequiredService<CalculateBonusJob>();
        RecurringJob.AddOrUpdate(
            "Calculate-Bonus",
            () => calculateBonusJob.ExecuteAsync(),
            calculateBonusJob.Cron,
            jobOptions);

        return app;
    }
}
