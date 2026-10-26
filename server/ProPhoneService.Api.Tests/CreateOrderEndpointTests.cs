using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ProPhoneService.Application.Contracts;
using ProPhoneService.Domain.Exceptions;
using ProPhoneService.Domain.Model;
using ProPhoneService.Domain.Repositories;

namespace ProPhoneService.Api.Tests;

public class CreateOrderEndpointTests
{
    /// <summary>
    /// Понедельник 2026-10-05 08:00 UTC = 12:00 по Самаре (рабочий день, рабочие часы)
    /// </summary>
    private static readonly DateTimeOffset _nowUtc = new(2026, 10, 5, 8, 0, 0, TimeSpan.Zero);

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Фиксированное "сейчас" для детерминированной валидации даты визита
    /// </summary>
    private class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private class CreateOrderHarness
    {
        public Service[] Services { get; init; } = [];

        public Client? ExistingClient { get; init; }

        /// <summary>
        /// Клиент, который «появляется» в базе после первой неудачной вставки (гонка)
        /// </summary>
        public Client? RaceWinner { get; init; }

        public bool UniqueViolationOnFirstSave { get; init; }

        public RepairOrder? CreatedOrder { get; private set; }

        public Client? CreatedClient { get; private set; }

        public Device? CreatedDevice { get; private set; }

        public WebApplicationFactory<Program> CreateFactory()
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
            {
                host.ConfigureTestServices(services =>
                {
                    var clientRepo = new Mock<IClientRepository>();
                    var phoneLookups = 0;
                    clientRepo
                        .Setup(r => r.GetByPhoneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((string phone, CancellationToken _) =>
                        {
                            phoneLookups++;
                            return RaceWinner is not null && phoneLookups > 1 ? RaceWinner : ExistingClient;
                        });
                    clientRepo
                        .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((string email, CancellationToken _) => ExistingClient);
                    clientRepo
                        .Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                        .Callback((Client c, CancellationToken _) => CreatedClient = c)
                        .Returns(Task.CompletedTask);

                    var deviceRepo = new Mock<IRepository<Device>>();
                    deviceRepo
                        .Setup(r => r.AddAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
                        .Callback((Device d, CancellationToken _) => CreatedDevice = d)
                        .Returns(Task.CompletedTask);

                    var serviceRepo = new Mock<IServiceRepository>();
                    serviceRepo
                        .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((IReadOnlyCollection<Guid> ids, CancellationToken _) =>
                            Services.Where(s => ids.Contains(s.Id)).ToList());

                    var orderRepo = new Mock<IRepairOrderRepository>();
                    orderRepo
                        .Setup(r => r.AddAsync(It.IsAny<RepairOrder>(), It.IsAny<CancellationToken>()))
                        .Callback((RepairOrder o, CancellationToken _) => CreatedOrder = o)
                        .Returns(Task.CompletedTask);
                    var savedCalls = 0;
                    orderRepo
                        .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .Callback(() => savedCalls++)
                        .ReturnsAsync(() =>
                        {
                            if (UniqueViolationOnFirstSave && savedCalls == 1)
                            {
                                throw new UniqueConstraintViolationException(
                                    "Дубликат контакта", new Exception());
                            }

                            return 1;
                        });

                    services.RemoveAll<IClientRepository>();
                    services.AddScoped(_ => clientRepo.Object);
                    services.RemoveAll<IRepository<Device>>();
                    services.AddScoped(_ => deviceRepo.Object);
                    services.RemoveAll<IServiceRepository>();
                    services.AddScoped(_ => serviceRepo.Object);
                    services.RemoveAll<IRepairOrderRepository>();
                    services.AddScoped(_ => orderRepo.Object);

                    services.RemoveAll<TimeProvider>();
                    services.AddSingleton<TimeProvider>(new FixedTimeProvider(_nowUtc));
                });
            });
        }
    }

    private static async Task<Dictionary<string, JsonElement>> ReadErrorsAsync(HttpResponseMessage response)
    {
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var errors = new Dictionary<string, JsonElement>();
        if (doc is not null && doc.RootElement.TryGetProperty("errors", out var errorsNode))
        {
            foreach (var property in errorsNode.EnumerateObject())
            {
                errors[property.Name] = property.Value;
            }
        }

        return errors;
    }

    private static object ValidBody(Guid[] serviceIds) => new
    {
        name = "Иван",
        phone = "+79001234567",
        email = (string?)null,
        deviceType = "Phone",
        deviceManufacturer = "Apple",
        deviceModel = "iPhone 13",
        preferredVisitAt = "2026-10-07T12:00:00+04:00",
        serviceIds,
    };

    [Fact]
    public async Task Create_ValidRequest_Returns201WithOrderNumberAndSnapshotPrice()
    {
        var charging = new Service { Id = Guid.NewGuid(), Name = "Замена разъема зарядки", Price = 1200m, IsActive = true };
        var battery = new Service { Id = Guid.NewGuid(), Name = "Замена аккумулятора", Price = 1500m, IsActive = true };
        var harness = new CreateOrderHarness { Services = [charging, battery] };
        using var factory = harness.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", ValidBody([charging.Id, battery.Id]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);

        Assert.NotNull(harness.CreatedOrder);
        Assert.Equal(2700m, harness.CreatedOrder.TotalPrice);
        Assert.Equal(2, harness.CreatedOrder.Services.Count);

        Assert.NotNull(harness.CreatedClient);
        Assert.Equal("Иван", harness.CreatedClient.Name);
        Assert.Equal("+79001234567", harness.CreatedClient.Phone);

        Assert.NotNull(harness.CreatedDevice);
        Assert.Equal("Apple", harness.CreatedDevice.Manufacturer);
        Assert.Equal("iPhone 13", harness.CreatedDevice.Model);
    }

    [Fact]
    public async Task Create_InvalidPhone_Returns400WithPhoneError()
    {
        using var factory = new CreateOrderHarness().CreateFactory();
        using var client = factory.CreateClient();

        var body = new
        {
            name = "Иван",
            phone = "12345",
            deviceType = "Phone",
            deviceManufacturer = "Apple",
            deviceModel = "iPhone 13",
            preferredVisitAt = "2026-10-07T12:00:00+04:00",
        };

        var response = await client.PostAsJsonAsync("/api/orders", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.True(errors.ContainsKey("Phone"));
    }

    [Fact]
    public async Task Create_PastVisitDate_Returns400WithVisitError()
    {
        using var factory = new CreateOrderHarness().CreateFactory();
        using var client = factory.CreateClient();

        var body = new
        {
            name = "Иван",
            phone = "+79001234567",
            deviceType = "Phone",
            deviceManufacturer = "Apple",
            deviceModel = "iPhone 13",
            preferredVisitAt = "2026-10-04T12:00:00+04:00",
        };

        var response = await client.PostAsJsonAsync("/api/orders", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.True(errors.ContainsKey("PreferredVisitAt"));
    }

    [Fact]
    public async Task Create_UnknownService_Returns400WithServiceError()
    {
        using var factory = new CreateOrderHarness().CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", ValidBody([Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.True(errors.ContainsKey("ServiceIds"));
    }

    [Fact]
    public async Task Create_NoPhoneAndNoEmail_Returns400WithContactError()
    {
        using var factory = new CreateOrderHarness().CreateFactory();
        using var client = factory.CreateClient();

        var body = new
        {
            name = "Иван",
            phone = (string?)null,
            email = (string?)null,
            deviceType = "Phone",
            deviceManufacturer = "Apple",
            deviceModel = "iPhone 13",
            preferredVisitAt = "2026-10-07T12:00:00+04:00",
        };

        var response = await client.PostAsJsonAsync("/api/orders", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.True(errors.ContainsKey("Phone"));
    }

    [Fact]
    public async Task Create_OnlyEmail_Returns201()
    {
        using var factory = new CreateOrderHarness().CreateFactory();
        using var client = factory.CreateClient();

        var body = new
        {
            name = "Иван",
            phone = (string?)null,
            email = "ivan@example.com",
            deviceType = "Phone",
            deviceManufacturer = "Apple",
            deviceModel = "iPhone 13",
            preferredVisitAt = "2026-10-07T12:00:00+04:00",
        };

        var response = await client.PostAsJsonAsync("/api/orders", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ExistingPhone_ReusesClientWithoutDuplicate()
    {
        var existing = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Старый клиент",
            Phone = "+79001234567",
            Email = null,
        };
        var harness = new CreateOrderHarness { ExistingClient = existing };
        using var factory = harness.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", ValidBody([]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Существующий клиент переиспользован: заказ ссылается на его Id, новый клиент не создавался
        Assert.NotNull(harness.CreatedOrder);
        Assert.Equal(existing.Id, harness.CreatedOrder.ClientId);
        Assert.Null(harness.CreatedClient);
    }

    [Fact]
    public async Task Create_DuplicateContactRace_RetriesWithWinnerAndReturns201()
    {
        var winner = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Победитель гонки",
            Phone = "+79001234567",
            Email = null,
        };
        var harness = new CreateOrderHarness
        {
            UniqueViolationOnFirstSave = true,
            RaceWinner = winner,
        };
        using var factory = harness.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", ValidBody([]));

        // Конфликт уникальности на первой вставке -> победитель подтягивается, заявка создаётся на его Id
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(harness.CreatedOrder);
        Assert.Equal(winner.Id, harness.CreatedOrder.ClientId);
        Assert.NotEqual(winner.Id, harness.CreatedOrder.Id);
    }
}
