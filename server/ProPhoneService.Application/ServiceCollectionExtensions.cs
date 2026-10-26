using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using ProPhoneService.Application.Mapper;
using ProPhoneService.Application.Services;

namespace ProPhoneService.Application;

/// <summary>
/// Регистрация сервисов прикладного слоя в DI-контейнере
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить сервисы прикладного слоя, кэш прайс-листа и Mapster
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHybridCache();
        services.AddScoped<PriceListService>();
        services.AddScoped<OrderWorkflowService>();
        services.AddScoped<OrderCreationService>();
        services.AddSingleton(TimeProvider.System);

        var config = new TypeAdapterConfig();
        config.Scan(typeof(MappingRegister).Assembly);

        services.AddSingleton(config);
        services.AddSingleton<IMapper, ServiceMapper>();

        return services;
    }
}
