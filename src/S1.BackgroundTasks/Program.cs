using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Hangfire;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Builder;
using S1.BackgroundTasks;

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

var app = builder.Build();
app.UseHangfireDashboard(string.Empty);
app.Run();
