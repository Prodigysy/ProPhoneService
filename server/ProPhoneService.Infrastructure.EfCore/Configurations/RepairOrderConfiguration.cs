using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProPhoneService.Domain.Model;

namespace ProPhoneService.Infrastructure.EfCore.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="RepairOrder"/>
/// </summary>
public class RepairOrderConfiguration : IEntityTypeConfiguration<RepairOrder>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RepairOrder> builder)
    {
        // Токен конкурентности: системная колонка PostgreSQL xmin (id транзакции),
        // меняется при каждом UPDATE — защита от потерянных апдейтов
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();

        builder.Property(x => x.Status).HasConversion<string>();

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.DeviceId);
        // Составной индекс под запрос панели мастера: фильтр по статусу + сортировка по дате
        builder.HasIndex(x => new { x.Status, x.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(x => x.PreferredVisitAt);

        builder
            .HasMany(x => x.StatusHistory)
            .WithOne(x => x.RepairOrder)
            .HasForeignKey(x => x.RepairOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Services)
            .WithOne(x => x.RepairOrder)
            .HasForeignKey(x => x.RepairOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
