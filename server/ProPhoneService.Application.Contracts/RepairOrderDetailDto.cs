using ProPhoneService.Domain.Shared.Enum;

namespace ProPhoneService.Application.Contracts;

/// <summary>
/// Публичное представление детальной карточки заказа
/// </summary>
/// <param name="Id">Номер заказа</param>
/// <param name="Status">Текущий статус заказа</param>
/// <param name="Device">Устройство в ремонте</param>
/// <param name="Client">Клиент, оформивший заказ</param>
/// <param name="Services">Услуги, включённые в заказ</param>
/// <param name="TotalPrice">Итоговая стоимость ремонта, руб</param>
/// <param name="PreferredVisitAt">Желаемая дата и время визита (UTC, может отсутствовать)</param>
/// <param name="CreatedAt">Дата и время создания заказа (UTC)</param>
/// <param name="StatusTimeline">Тайм-лайн смены статусов заказа</param>
public record RepairOrderDetailDto(
    Guid Id,
    RepairStatus? Status,
    DeviceDto Device,
    ClientDto Client,
    IReadOnlyList<RepairOrderServiceDto> Services,
    decimal TotalPrice,
    DateTime? PreferredVisitAt,
    DateTime CreatedAt,
    IReadOnlyList<RepairStatusHistoryDto> StatusTimeline);
