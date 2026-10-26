using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProPhoneService.Domain.Model;

namespace ProPhoneService.Infrastructure.EfCore.Auditing;

/// <summary>
/// Заполнение служебных полей при сохранении:
/// <c>CreatedAt</c> - момент добавления сущности, <c>ChangedAt</c> - момент записи истории статуса;
/// Время берётся из единого источника - <see cref="TimeProvider"/>, значения проставляются
/// в том же батче, что и сами изменения (без отдельной таблицы аудита)
/// </summary>
public class AuditInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void StampTimestamps(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in context.ChangeTracker.Entries()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name == nameof(RepairOrder.CreatedAt) && entry.State == EntityState.Added)
                {
                    property.CurrentValue = now;
                }
                else if (property.Metadata.Name == nameof(RepairStatusHistory.ChangedAt)
                         && entry.Entity is RepairStatusHistory)
                {
                    property.CurrentValue = now;
                }
            }
        }
    }
}
