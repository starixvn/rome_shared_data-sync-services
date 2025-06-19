using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Shared.Extensions;
using CashServiceBilling.Extensions;
using Hangfire;
using Hangfire.MemoryStorage;
using CashServiceBilling.Jobs;

namespace CashServiceBilling;

[ExcludeFromCodeCoverage]
public sealed class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
        => this.configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddONEBookUM(configuration)
            .AddFanexLogging(configuration)
            .AddFanexDataWithSqlServer(configuration)
            .AddApplicationServices(configuration)
            .AddHealthChecks();

        // Add Hangfire services with in-memory storage
        services.AddHangfire(config => config.UseMemoryStorage());
        services.AddHangfireServer();
    }

    public void Configure(IApplicationBuilder app)
    {
        app
            .UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[]
                {
                    new Authorization.BasicAuthAuthorizationFilter(configuration)
                }
            }) // Configure Hangfire dashboard (optional)
            .UseRouting()
            .UseEndpoints(endpoints => endpoints.MapHealthChecks("/"));

        var accumulatedCreditJob = app.ApplicationServices.GetRequiredService<AccumulatedCreditJob>();
        RecurringJob.AddOrUpdate(
            "Returning-Accumulated-Credit-To-Agent",
            () => accumulatedCreditJob.ExecuteAsync(),
            accumulatedCreditJob.Cron,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
    }
}
