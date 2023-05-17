using System.Diagnostics.CodeAnalysis;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Fanex.Logging.Sentry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;
using ONEbook.UM.Transport;
using Sentry;
using Shared.Services;

namespace Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class FanexLibsExtensions
{
    public static IServiceCollection AddONEBookUM(this IServiceCollection services, IConfiguration configuration)
    {
        var umConfig = new UMClientConfiguration().Initialize(configuration);

        return services
            .AddSingleton(umConfig)
            .AddSingleton<IMaintenanceService, MaintenanceService>()
            .AddSingleton<IUMCacheService, UMCacheService>()
            .AddSingleton<IUMRestClient, UMRestClient>();
    }

    public static IServiceCollection AddFanexLogging(this IServiceCollection services, IConfiguration configuration)
    {
        LogManager
            .SetDefaultLogCategory(configuration["Fanex.Logging:DefaultCategory"])
            .Use(GetLogEngines(configuration));

        return services
            .AddSingleton(Logger.Log)
            .AddSingleton<ILoggerService, LoggerService>();
    }

    private static IEnumerable<ILogEngine> GetLogEngines(IConfiguration configuration)
    {
        var rabbitMQConnectionStrings = configuration["Fanex.Logging:RabbitMQConnectionStrings"];
        if (!string.IsNullOrWhiteSpace(rabbitMQConnectionStrings))
        {
            yield return new RabbitMQLogging(rabbitMQConnectionStrings);
        }

        var sentryUrl = configuration["Fanex.Logging:SentryDsn"];
        if (!string.IsNullOrWhiteSpace(sentryUrl))
        {
            yield return new SentryLogging(new SentryEngineOptions
            {
                Dsn = new Dsn(sentryUrl)
            });
        }
    }

    public static IServiceCollection AddFanexDataWithSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration
            .GetSection("ConnectionStrings")
            .GetChildren()
            .ToDictionary(
                connection => connection.Key,
                connection => new ConnectionConfiguration(connection.Key, connection.Value), StringComparer.OrdinalIgnoreCase);

        DbSettingProviderManager
            .StartNewSession()
            .WithSqlServer("App_Data/SpNames.xml")
            .Use(connectionStrings)
            .Run();

        return services.AddSingleton<IDynamicRepository, DynamicRepository>();
    }

    public static IServiceCollection AddFanexCachingMemory(this IServiceCollection services)
        => services.AddSingleton<Fanex.Caching.ICacheService, Fanex.Caching.CacheService>();
}
