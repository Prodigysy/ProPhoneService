using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProPhoneService.Domain.Model;
using ProPhoneService.Domain.Repositories;
using ProPhoneService.Infrastructure.EfCore;
using ProPhoneService.Infrastructure.EfCore.Repositories;

namespace ProPhoneService.Api.Tests;

public class RepositoryRegistrationTests
{
    [Fact]
    public void AddRepositories_ResolvesRepositories()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddDbContext<ProPhoneServiceDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=prophoneservice_test"));
        services.AddRepositories();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var clientRepository = scope.ServiceProvider.GetRequiredService<IRepository<Client>>();
        Assert.IsType<EfRepository<Client>>(clientRepository);

        var clientRepositorySpecialized = scope.ServiceProvider.GetRequiredService<IClientRepository>();
        Assert.IsType<ClientRepository>(clientRepositorySpecialized);

        var repairOrderRepository = scope.ServiceProvider.GetRequiredService<IRepairOrderRepository>();
        Assert.IsType<RepairOrderRepository>(repairOrderRepository);

        var serviceRepository = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
        Assert.IsType<ServiceRepository>(serviceRepository);
    }

    [Fact]
    public async Task GetAllAsync_RejectsInvalidPage()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddDbContext<ProPhoneServiceDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=prophoneservice_test"));
        services.AddRepositories();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Client>>();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetAllAsync(skip: -1, take: 50));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetAllAsync(skip: 0, take: 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetAllAsync(skip: 0, take: 101));
    }
}
