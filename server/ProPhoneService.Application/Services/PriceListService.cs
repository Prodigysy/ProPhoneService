using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;
using ProPhoneService.Application.Contracts;
using ProPhoneService.Domain.Repositories;

namespace ProPhoneService.Application.Services;

/// <summary>
/// Прайс-лист мастерской: чтение активных услуг с кэшированием
/// </summary>
public class PriceListService(
    IServiceRepository serviceRepository,
    IMapper mapper,
    HybridCache cache)
{
    /// <summary>
    /// Ключ кэша списка услуг
    /// </summary>
    public const string CacheKey = "services:active";

    /// <summary>
    /// Тег для инвалидации кэша прайс-листа (задел на правку прайса в спринте 2)
    /// </summary>
    public const string CacheTag = "services";

    private static readonly HybridCacheEntryOptions _cacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(5),
    };

    /// <summary>
    /// Получить активные услуги прайс-листа (из кэша, при промахе - из БД)
    /// </summary>
    public async Task<IReadOnlyList<ServiceDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            CacheKey,
            async token => await LoadAsync(token),
            _cacheOptions,
            [CacheTag],
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Сбросить кэш прайс-листа (вызывается после правки услуг)
    /// </summary>
    public ValueTask InvalidateAsync(CancellationToken cancellationToken = default)
    {
        return cache.RemoveByTagAsync(CacheTag, cancellationToken);
    }

    private async Task<ServiceDto[]> LoadAsync(CancellationToken cancellationToken)
    {
        var services = await serviceRepository.GetAllActiveAsync(cancellationToken);

        return mapper.Map<ServiceDto[]>(services);
    }
}
