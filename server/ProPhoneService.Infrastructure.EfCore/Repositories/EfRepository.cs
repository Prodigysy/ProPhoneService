using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProPhoneService.Domain.Exceptions;
using ProPhoneService.Domain.Model;
using ProPhoneService.Domain.Repositories;

namespace ProPhoneService.Infrastructure.EfCore.Repositories;

/// <summary>
/// Универсальная EF Core реализация <see cref="IRepository{T}"/>
/// </summary>
/// <typeparam name="T">Тип сущности с ключом <c>Guid Id</c></typeparam>
public class EfRepository<T>(ProPhoneServiceDbContext context) : IRepository<T>
    where T : class, IEntity
{
    /// <summary>
    /// Максимальный размер страницы для <see cref="GetAllAsync"/>
    /// </summary>
    protected const int MaxPageSize = 100;

    /// <summary>
    /// Набор сущностей для текущего типа
    /// </summary>
    protected DbSet<T> Set => context.Set<T>();

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        int skip = 0,
        int take = 25,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(skip, take);

        return await Set
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(T entity)
    {
        Set.Update(entity);
    }

    /// <inheritdoc />
    public virtual void Remove(T entity)
    {
        Set.Remove(entity);
    }

    /// <inheritdoc />
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Переводим EF-специфичное исключение в доменное: прикладной слой не зависит от EF Core
            throw new ConcurrencyConflictException("Сущность была изменена другим процессом", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException("Нарушено ограничение уникальности", ex);
        }
    }

    /// <summary>
    /// Проверить, что цепочка исключений указывает на нарушение ограничения уникальности
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        for (var current = (Exception?)ex; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Проверка корректности параметров пагинации
    /// </summary>
    protected static void ValidatePage(int skip, int take)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), skip, "Skip не может быть отрицательным");
        }

        if (take is < 1 or > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take, $"Take должен быть в диапазоне от 1 до {MaxPageSize}");
        }
    }
}
