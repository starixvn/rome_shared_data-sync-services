using System.Net;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Hangfire;
using Hangfire.InMemory;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using S1.BackgroundTasks;
using S1.BackgroundTasks.Services;

var builder = WebApplication.CreateBuilder(args);

LogManager
   .SetDefaultLogCategory(builder.Configuration["Fanex.Logging:DefaultCategory"])
   .Use(
   [
        new RabbitMQLogging(builder.Configuration["Fanex.Logging:RabbitMQConnectionStrings"])
   ]);

var appName = typeof(Program).Assembly.GetName().Name;

try
{
    Logger.Log.Info($"Starting {appName}");
    builder.Services.AddSingleton(Logger.Log);

    builder.Services.AddFanexSql(builder.Configuration);
    builder.Services.AddSingleton<IDynamicRepository, DynamicRepository>();
    builder.Services.AddSingleton<IWinLossImportService, WinLossImportService>();

    builder.Services.AddHttpClient<IPlayerWinLossService, PlayerWinLossService>(httpClient =>
    {
        httpClient.BaseAddress = new Uri(builder.Configuration.GetValue<string>("S1:BaseUrl"));
    })
    .ConfigurePrimaryHttpMessageHandler(_ =>
    {
        var httpClientHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
        var proxyUrl = builder.Configuration.GetValue<string>("S1:Proxy");
        if (!string.IsNullOrWhiteSpace(proxyUrl))
        {
            var proxy = new WebProxy
            {
                Address = new Uri(proxyUrl),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
            };
            httpClientHandler.Proxy = proxy;
            httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        return httpClientHandler;
    });

    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseIgnoredAssemblyVersionTypeResolver()
        .UseInMemoryStorage(new InMemoryStorageOptions
        {
            IdType = InMemoryStorageIdType.Long
        }));

    builder.Services.AddHangfireServer();

    builder.Services.AddHostedService<Worker>();

    builder.Services.AddHealthChecks();

    var app = builder.Build();
    app.UseRouting();
    app.UseEndpoints(endpoints => endpoints.MapHealthChecks("/healthz"));

    var options = new DashboardOptions
    {
        Authorization = builder.Configuration.GetSection("Users").GetChildren().Select(x => new HangfireCustomBasicAuthenticationFilter
        {
            User = x.GetValue<string>("Username"),
            Pass = x.GetValue<string>("Password")
        })
    };
    app.UseHangfireDashboard("/hangfire", options);
    app.Run();
    Logger.Log.Info($"{appName} has been started up.");
}
catch (Exception ex)
{
    Logger.Log.Error($"Program terminated unexpectedly {appName}", ex);
}

