using Microsoft.AspNetCore.Hosting;
using Serilog;
using CashServiceBilling;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    await Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services))
        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
        .Build()
        .RunAsync();
}
catch (Exception e)
{
    Console.WriteLine(e);
}