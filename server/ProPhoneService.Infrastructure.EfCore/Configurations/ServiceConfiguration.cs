using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProPhoneService.Domain.Model;

namespace ProPhoneService.Infrastructure.EfCore.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="Service"/>
/// </summary>
public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasIndex(x => x.IsActive);

        builder
            .HasMany(x => x.RepairOrders)
            .WithOne(x => x.Service)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        SeedPriceList(builder);
    }

    /// <summary>
    /// Стартовый прайс-лист мастерской
    /// </summary>
    private static void SeedPriceList(EntityTypeBuilder<Service> builder)
    {
        builder.HasData(
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e01"), Name = "Диагностика (выявление неисправности)", Price = 0m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e02"), Name = "Замена дисплейного модуля", Price = 1500m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e03"), Name = "Замена разъема зарядки", Price = 1200m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e04"), Name = "Замена аккумулятора", Price = 1500m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e05"), Name = "Замена микрофона", Price = 1500m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e06"), Name = "Замена динамика", Price = 1100m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e07"), Name = "Замена кнопки", Price = 1200m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e08"), Name = "Замена камеры", Price = 1500m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e09"), Name = "Профилактика", Price = 1000m, IsActive = true },
            new Service { Id = Guid.Parse("6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e10"), Name = "Чистка сеток и разъемов", Price = 700m, IsActive = true });
    }
}
