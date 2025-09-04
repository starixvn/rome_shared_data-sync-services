using System.Diagnostics.CodeAnalysis;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.FanexData.Extensions;

[ExcludeFromCodeCoverage]
public static class FanexDataExtensions
{
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
}
