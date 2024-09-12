using System.Net;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Hangfire;
using Hangfire.InMemory;
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
builder.Services.AddSingleton(Logger.Log);

builder.Services.AddFanexSql(builder.Configuration);
builder.Services.AddSingleton<IDynamicRepository, DynamicRepository>();
builder.Services.AddSingleton<IWinLossImportService, WinLossImportService>();
builder.Services.AddHttpClient<IPlayerWinLossService, PlayerWinLossService>(httpClient => httpClient.BaseAddress = new Uri(builder.Configuration.GetValue<string>("S1:BaseUrl")))
    .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });

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
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()]
});
app.Run();
