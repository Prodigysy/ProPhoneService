using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ProPhoneService.Domain.Exceptions;
using ProPhoneService.Domain.Model;
using ProPhoneService.Domain.Repositories;
using ProPhoneService.Domain.Shared.Enum;

namespace ProPhoneService.Api.Tests;

public class ChangeStatusEndpointTests
{
    private class OrderStore
    {
        public RepairOrder? Order { get; set; }

        public bool ThrowConcurrency { get; init; }

        public int SavedCount { get; private set; }

        public WebApplicationFactory<Program> CreateFactory()
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
            {
                host.ConfigureTestServices(services =>
                {
                    var repository = new Mock<IRepairOrderRepository>();
                    repository
                        .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Guid id, CancellationToken _) => Order is { Id: var orderId } && orderId == id
                            ? Order
                            : null);

                    repository
                        .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .Callback(() => SavedCount++)
                        .ReturnsAsync(() =>
                        {
                            if (ThrowConcurrency)
                            {
                                throw new ConcurrencyConflictException("Изменён параллельно", new Exception());
                            }

                            return 1;
                        });

                    services.RemoveAll<IRepairOrderRepository>();
                    services.AddScoped(_ => repository.Object);
                });
            });
        }
    }

    private static RepairOrder NewOrder() =>
        RepairOrder.Create(Guid.NewGuid(), Guid.NewGuid(), preferredVisitAt: null);

    [Fact]
    public async Task ChangeStatus_ValidTransition_Returns204AndWritesHistory()
    {
        var order = NewOrder();
        var store = new OrderStore { Order = order };
        using var factory = store.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/api/orders/{order.Id}/status",
            new { status = "InDiagnostic", comment = "Взяли в диагностику" });

        Assert.Equal(RepairStatus.InDiagnostic, order.Status);
        var entry = Assert.Single(order.StatusHistory, h => h.Status == RepairStatus.InDiagnostic);
        Assert.Equal("Взяли в диагностику", entry.Comment);
        Assert.Equal(1, store.SavedCount);
    }

    [Fact]
    public async Task ChangeStatus_ForbiddenJump_Returns409()
    {
        var order = NewOrder(); // Created
        var store = new OrderStore { Order = order };
        using var factory = store.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/api/orders/{order.Id}/status",
            new { status = "Ready", comment = "Готово" }); // перепрыг Created -> Ready запрещён

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(RepairStatus.Created, order.Status);
        Assert.Equal(0, store.SavedCount);
    }

    [Fact]
    public async Task ChangeStatus_BackwardTransition_Returns409()
    {
        var order = NewOrder();
        order.ChangeStatus(RepairStatus.InDiagnostic);
        var store = new OrderStore { Order = order };
        using var factory = store.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/api/orders/{order.Id}/status",
            new { status = "Created", comment = (string?)null }); // назад тоже нельзя

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(RepairStatus.InDiagnostic, order.Status);
        Assert.Equal(0, store.SavedCount);
    }

    [Fact]
    public async Task ChangeStatus_UnknownOrder_Returns404()
    {
        var store = new OrderStore { Order = null };
        using var factory = store.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/api/orders/{Guid.NewGuid()}/status",
            new { status = "InDiagnostic", comment = (string?)null });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, store.SavedCount);
    }

    [Fact]
    public async Task ChangeStatus_TerminalStatus_Returns409()
    {
        var order = NewOrder();
        foreach (var next in new[] { RepairStatus.InDiagnostic, RepairStatus.Agreed, RepairStatus.InProgress })
        {
            order.ChangeStatus(next);
        }

        var store = new OrderStore { Order = order };
        using var factory = store.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/api/orders/{order.Id}/status",
            new { status = "Cancelled", comment = "Поздно" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        response = await client.PatchAsJsonAsync(
            $"/api/orders/{order.Id}/status",
            new { status = "InDiagnostic", comment = "Воскрешение" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(RepairStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task ChangeStatus_ConcurrencyConflict_Returns409()
    {
        var order = NewOrder();
        var store = new OrderStore { Order = order, ThrowConcurrency = true };
        using var factory = store.CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/api/orders/{order.Id}/status",
            new { status = "InDiagnostic", comment = (string?)null });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
