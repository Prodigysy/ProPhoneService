using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProPhoneService.Domain.Repositories;
using ProPhoneService.Infrastructure.EfCore.Auditing;
using ProPhoneService.Infrastructure.EfCore.Repositories;

namespace ProPhoneService.Infrastructure.EfCore;

/// <summary>
/// Регистрация репозиториев в DI-контейнере
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить репозитории, контекст базы данных и аудит изменений
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="connectionString">Строка подключения; без неё контекст настраивается отдельно (тесты)</param>
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        string? connectionString = null)
    {
        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<ProPhoneServiceDbContext>((sp, options) =>
        {
            if (connectionString is not null)
            {
                options.UseNpgsql(connectionString);
            }
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IRepairOrderRepository, RepairOrderRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        return services;
    }
}
