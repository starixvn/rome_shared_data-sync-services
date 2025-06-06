using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Shared.Extensions;
using CashServiceBilling.Extensions;
using CashServiceBilling.Models;

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

        services.Configure<JobsSettingsModel>(options => configuration.GetSection("JobsSettings").Bind(options));
    }

    public void Configure(IApplicationBuilder app)
        => app
            .UseRouting()
            .UseEndpoints(endpoints => endpoints.MapHealthChecks("/"));
}
