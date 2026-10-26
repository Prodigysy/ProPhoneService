using ProPhoneService.Domain.Shared.Enum;

namespace ProPhoneService.Application.Contracts;

/// <summary>
/// Публичное представление заказа на ремонт для списка панели мастера
/// </summary>
/// <param name="Id">Номер заказа (идентификатор)</param>
/// <param name="Status">Текущий статус заказа</param>
/// <param name="Device">Устройство в ремонте</param>
/// <param name="PreferredVisitAt">Желаемая дата и время визита (UTC, может отсутствовать)</param>
/// <param name="CreatedAt">Дата и время создания заказа (UTC)</param>
/// <param name="StatusTimeline">Тайм-лайн смены статусов заказа</param>
public record RepairOrderListItemDto(
    Guid Id,
    RepairStatus? Status,
    DeviceDto Device,
    DateTime? PreferredVisitAt,
    DateTime CreatedAt,
    IReadOnlyList<RepairStatusHistoryDto> StatusTimeline);
