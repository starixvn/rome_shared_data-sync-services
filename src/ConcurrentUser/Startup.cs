using System.Diagnostics.CodeAnalysis;
using ConcurrentUser.Extensions;
using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Shared.Extensions;
using Shared.Services;
using FanexVault.Services;
using Fanex.Data.Repository;

namespace ConcurrentUser;

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
            .AddFanexSql(configuration)
            .AddSingleton<IDynamicRepository, DynamicRepository>()
            .AddApplicationServices(configuration)
            .AddHealthChecks();

#pragma warning disable S2325
    public void Configure(IApplicationBuilder app)
#pragma warning restore S2325
        => app
            .UseRouting()
            .UseEndpoints(endpoints => endpoints.MapHealthChecks("/"));
}
