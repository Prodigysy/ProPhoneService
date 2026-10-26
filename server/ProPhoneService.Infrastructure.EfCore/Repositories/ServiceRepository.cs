using Microsoft.EntityFrameworkCore;
using ProPhoneService.Domain.Model;
using ProPhoneService.Domain.Repositories;

namespace ProPhoneService.Infrastructure.EfCore.Repositories;

/// <summary>
/// EF Core реализация <see cref="IServiceRepository"/>
/// </summary>
public class ServiceRepository(ProPhoneServiceDbContext context) : EfRepository<Service>(context), IServiceRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Service>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Set
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Service>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await Set
            .AsNoTracking()
            .Where(x => x.IsActive && ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
