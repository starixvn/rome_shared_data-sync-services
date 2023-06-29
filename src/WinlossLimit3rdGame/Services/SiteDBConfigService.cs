using WinlossLimit3rdGame.Criteria;
using Fanex.Data.Repository;
using Fanex.Caching;
using  WinlossLimit3rdGame.Model;

namespace WinlossLimit3rdGame.Services;

public interface ISiteDBConfigService
{
    Task<IEnumerable<SiteDBConfigModel>> GetAll(double cachedDay = 1);
}

public class SiteDBConfigService : ISiteDBConfigService
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly ICacheService cacheService;

    public SiteDBConfigService(IDynamicRepository dynamicRepository, ICacheService cacheService)
        => (this.dynamicRepository, this.cacheService) = (dynamicRepository, cacheService);

    public async Task<IEnumerable<SiteDBConfigModel>> GetAll(double cachedDay = 1)
    => await cacheService.GetOrSetAsync(
            "WinlossLimit3rdGame.SiteDBConfig",
            async () => await dynamicRepository.FetchAsync<SiteDBConfigModel>(new GetSiteDBConfigCriteria()),
            new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(cachedDay)));
}
