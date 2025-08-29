using System.Diagnostics.CodeAnalysis;
using BalanceRecord.Extensions;
using Microsoft.AspNetCore.Builder;
using Shared.Extensions;
using Shared.FanexData.Extensions;
using Shared.FanexData.Services;
using Shared.Services;

namespace BalanceRecord;

[ExcludeFromCodeCoverage]
public sealed class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
        => this.configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
        => services
            .AddONEBookUM(configuration)
            .AddFanexLogging(configuration)
            .AddSingleton<ILoggerService, LoggerService>()
            .AddFanexDataWithSqlServer(configuration)
            .AddFanexCachingMemory()
            .AddApplicationServices(configuration)
            .AddHealthChecks();

#pragma warning disable S2325
    public void Configure(IApplicationBuilder app)
#pragma warning restore S2325
        => app
            .UseRouting()
            .UseEndpoints(endpoints => endpoints.MapHealthChecks("/"));
}
