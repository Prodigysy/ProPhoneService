using ProPhoneService.Domain.Shared.Enum;

namespace ProPhoneService.Application.Contracts;

/// <summary>
/// Публичное представление записи истории смены статуса заказа
/// </summary>
/// <param name="Status">Статус, в который перешёл заказ</param>
/// <param name="ChangedAt">Дата и время смены статуса (UTC)</param>
/// <param name="Comment">Комментарий к смене статуса (может отсутствовать)</param>
public record RepairStatusHistoryDto(
    RepairStatus Status,
    DateTime ChangedAt,
    string? Comment);
