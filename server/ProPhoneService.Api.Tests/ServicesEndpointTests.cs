using Microsoft.AspNetCore.TestHost;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ProPhoneService.Application.Contracts;
using ProPhoneService.Application.Services;
using ProPhoneService.Domain.Model;
using ProPhoneService.Domain.Repositories;

namespace ProPhoneService.Api.Tests;

public class ServicesEndpointTests
{
    private static readonly Service[] _seedServices =
    [
        new() { Id = Guid.NewGuid(), Name = "Диагностика (выявление неисправности)", Price = 0m, IsActive = true },
        new() { Id = Guid.NewGuid(), Name = "Замена дисплейного модуля", Price = 1500m, IsActive = true },
        new() { Id = Guid.NewGuid(), Name = "Замена аккумулятора", Price = 1500m, IsActive = true },
    ];

    private static Service[] CurrentServices() => [.. _seedServices];

    /// <summary>
    /// Фабрика с подменённым IServiceRepository: источник данных, который можно мутировать между вызовами,
    /// со счётчиком обращений к источнику (для проверки кэш-попаданий)
    /// </summary>
    private class MutablePriceSource(Service[] services)
    {
        public Service[] Services { get; set; } = services;

        public int RepositoryCalls { get; private set; }

        public WebApplicationFactory<Program> CreateFactory()
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
            {
                host.ConfigureTestServices(servicesCollection =>
                {
                    var repository = new Mock<IServiceRepository>();
                    repository
                        .Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() =>
                        {
                            RepositoryCalls++;
                            return Services;
                        });

                    servicesCollection.RemoveAll<IServiceRepository>();
                    servicesCollection.AddScoped(_ => repository.Object);
                });
            });
        }
    }

    [Fact]
    public async Task GetAll_Returns200WithSeededServices()
    {
        using var factory = new MutablePriceSource(CurrentServices()).CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/services");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var services = await response.Content.ReadFromJsonAsync<List<ServiceDto>>();
        Assert.NotNull(services);
        Assert.Equal(3, services.Count);
        Assert.All(services, s => Assert.NotEqual(Guid.Empty, s.Id));
        Assert.Contains(services, s => s.Name == "Диагностика (выявление неисправности)" && s.Price == 0m);
        Assert.Contains(services, s => s.Name == "Замена дисплейного модуля" && s.Price == 1500m);
    }

    [Fact]
    public async Task GetAll_CachesAfterFirstCall_RepositoryHitOnceForTwoRequests()
    {
        var source = new MutablePriceSource(CurrentServices());
        using var factory = source.CreateFactory();
        using var client = factory.CreateClient();

        var first = await client.GetAsync("/api/services");
        var second = await client.GetAsync("/api/services");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        // Второй запрос обслужен из кэша: источник данных дёрган ровно один раз
        Assert.Equal(1, source.RepositoryCalls);
    }

    [Fact]
    public async Task GetAll_AfterSourceChange_StillReturnsStaleData_UntilInvalidated()
    {
        var source = new MutablePriceSource(CurrentServices());
        using var factory = source.CreateFactory();
        using var client = factory.CreateClient();

        // Первый запрос — прогрев кэша
        var first = await client.GetAsync("/api/services");
        var firstList = await first.Content.ReadFromJsonAsync<List<ServiceDto>>();
        Assert.Equal(3, firstList!.Count);

        // Источник меняется (мастер отредактировал прайс напрямую в БД)
        source.Services =
        [
            new() { Id = Guid.NewGuid(), Name = "Срочный ремонт", Price = 9999m, IsActive = true },
        ];

        // Без инвалидации кэш отдаёт старые данные — TTL 5 минут ещё не истёк
        var stale = await client.GetAsync("/api/services");
        var staleList = await stale.Content.ReadFromJsonAsync<List<ServiceDto>>();
        Assert.Equal(3, staleList!.Count);
        Assert.DoesNotContain(staleList, s => s.Name == "Срочный ремонт");

        // Инвалидация по тегу — следующий запрос уходит в источник и видит новое
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var priceList = scope.ServiceProvider.GetRequiredService<PriceListService>();
            await priceList.InvalidateAsync();
        }

        var fresh = await client.GetAsync("/api/services");
        var freshList = await fresh.Content.ReadFromJsonAsync<List<ServiceDto>>();
        Assert.Single(freshList!);
        Assert.Equal("Срочный ремонт", freshList![0].Name);
        Assert.Equal(9999m, freshList[0].Price);
    }
}
