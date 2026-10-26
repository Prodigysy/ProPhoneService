using Microsoft.Extensions.Options;
using ProPhoneService.Application.Contracts;
using ProPhoneService.Domain.Exceptions;
using ProPhoneService.Domain.Model;
using ProPhoneService.Domain.Repositories;
using ProPhoneService.Domain.Services;
using ProPhoneService.Domain.Shared;

namespace ProPhoneService.Application.Services;

/// <summary>
/// Создание заявки на ремонт: клиент + устройство + заказ + выбранные услуги
/// </summary>
public class OrderCreationService(
    IClientRepository clientRepository,
    IRepository<Device> deviceRepository,
    IServiceRepository serviceRepository,
    IRepairOrderRepository orderRepository,
    TimeProvider timeProvider,
    IOptions<WorkshopOptions> workshopOptions)
{
    /// <summary>
    /// Создать заявку на ремонт
    /// </summary>
    /// <param name="request">Данные заявки</param>
    /// <returns>Номер заявки или ошибки валидации по полям</returns>
    public async Task<CreateOrderResult> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();

        // Желаемая дата и время визита: конвертация в UTC + проверка рабочих часов
        var preferredVisitAtUtc = request.PreferredVisitAt!.Value.UtcDateTime;
        var visitError = VisitTimeValidator.Validate(
            preferredVisitAtUtc,
            timeProvider.GetUtcNow().UtcDateTime,
            workshopOptions.Value);
        if (visitError is not null)
        {
            errors["PreferredVisitAt"] = [visitError];
        }

        // Контакт: телефон или email обязательно
        var hasPhone = !string.IsNullOrWhiteSpace(request.Phone);
        var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
        if (!hasPhone && !hasEmail)
        {
            errors["Phone"] = ["Укажите телефон или email"];
        }

        decimal totalPrice = 0;
        List<Service> selectedServices = [];
        var serviceIds = request.ServiceIds?.Distinct().ToArray() ?? [];
        if (serviceIds.Length > 0)
        {
            var found = await serviceRepository.GetByIdsAsync(serviceIds, cancellationToken);
            var foundIds = found.Select(s => s.Id).ToHashSet();
            var missing = serviceIds.Where(id => !foundIds.Contains(id)).ToArray();

            if (missing.Length > 0)
            {
                errors["ServiceIds"] = [$"Не найдены активные услуги: {string.Join(", ", missing)}"];
            }

            selectedServices = [.. found];
            totalPrice = found.Sum(s => s.Price);
        }

        if (errors.Count > 0)
        {
            return CreateOrderResult.Failure(errors);
        }

        // Устройство и заявка пересоздаются на повторе: после неудачного сохранения
        // конфликтную вставку клиента нужно отвязать (Remove на сущности в состоянии Added отцепляет её)
        const int maxAttempts = 2;
        for (var attempt = 1; ; attempt++)
        {
            // Найти клиента по телефону (приоритет) или по email; иначе создать нового
            var existingClient = hasPhone
                ? await clientRepository.GetByPhoneAsync(request.Phone!, cancellationToken)
                : null;

            if (existingClient is null && hasEmail)
            {
                existingClient = await clientRepository.GetByEmailAsync(request.Email!, cancellationToken);
            }

            var client = existingClient ?? new Client
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
            };

            var device = new Device
            {
                Id = Guid.NewGuid(),
                Type = request.DeviceType!.Value,
                Manufacturer = request.DeviceManufacturer,
                Model = request.DeviceModel,
            };

            var order = RepairOrder.Create(
                client.Id,
                device.Id,
                preferredVisitAtUtc);
            order.TotalPrice = totalPrice;

            foreach (var service in selectedServices)
            {
                order.Services.Add(new RepairOrderService
                {
                    RepairOrderId = order.Id,
                    ServiceId = service.Id,
                    Price = service.Price,
                });
            }

            if (existingClient is null)
            {
                await clientRepository.AddAsync(client, cancellationToken);
            }

            await deviceRepository.AddAsync(device, cancellationToken);
            await orderRepository.AddAsync(order, cancellationToken);

            try
            {
                await orderRepository.SaveChangesAsync(cancellationToken);
                return CreateOrderResult.Success(order.Id);
            }
            catch (UniqueConstraintViolationException) when (existingClient is null && attempt < maxAttempts)
            {
                // Гонка: параллельный запрос уже вставил клиента с таким же контактом;
                // отвязываем неудачные вставки и на следующем шаге переиспользуем победителя
                clientRepository.Remove(client);
                deviceRepository.Remove(device);
                orderRepository.Remove(order);
            }
        }
    }
}
