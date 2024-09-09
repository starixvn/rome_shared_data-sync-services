using Hangfire;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Builder;
using S1.BackgroundTasks;

var builder = WebApplication.CreateBuilder(args);

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
