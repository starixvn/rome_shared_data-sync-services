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
using System.Xml;
using WinlossLimit3rdGame.Services;

namespace Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class FanexDataExtensions
{

    public static IServiceCollection CustomizeFanexDataWithSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        var xml = new XmlDocument();
        xml.Load("App_Data/SiteDB.bodb02.bodbAdminNet.xml");
        var node = xml.ChildNodes[0];
        node.RemoveAll();
        var passConnection = configuration.GetValue<string>("PasswordDB");
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<ISiteDBConfigService, SiteDBConfigService>();
        var siteDBService = services.BuildServiceProvider().GetRequiredService<ISiteDBConfigService>();

        var connections = configuration
            .GetSection("ConnectionStrings")
            .GetChildren()
            .ToDictionary(
                connection => connection.Key,
                connection => new ConnectionConfiguration(connection.Key, connection.Value),
                StringComparer.OrdinalIgnoreCase);

        DbSettingProviderManager
            .StartNewSession()
            .WithSqlServer("App_Data/SpNames.xml")
            .Use(connections)
            .Run();

        var siteDBs = siteDBService.GetAll().GetAwaiter().GetResult();
        siteDBs.ToList().ForEach(siteDB =>
        {
            NewDBSetting(xml, node, siteDB.LinkedServerName, configuration.GetValue<string>("SpNameOnSiteDB"));
            var key = $"{siteDB.LinkedServerName}.bodb02.bodbAdminNet";
            var value = $"Data Source={siteDB.AGIP};Persist Security Info=True;User ID=bodbAdminNet;Initial Catalog=bodb02;Password={passConnection}";
            connections.Add(key, new ConnectionConfiguration(key, value));
        });

        xml.Save("App_Data/SiteDB.bodb02.bodbAdminNet.xml");

        DbSettingProviderManager
         .StartNewSession()
         .Use(connections)
         .UseDefaultDbSettingProvider("~/App_Data")
         .Run();

        return services;
    }

    private static void NewDBSetting(XmlDocument xml, XmlNode node, string db, string spName)
    {
        var key = $"{db}_Task_Chatbot_CustBalanceGameSel";
        var newNode = xml.CreateElement("Setting");
        newNode.SetAttribute("Key", key);
        newNode.SetAttribute("SpName", spName);
        newNode.SetAttribute("DbName", $"{db}.bodb02.bodbAdminNet");
        newNode.SetAttribute("Timeout", "0");
        node.AppendChild(newNode);
    }
}
