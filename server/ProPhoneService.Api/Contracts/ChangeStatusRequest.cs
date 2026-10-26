using ProPhoneService.Domain.Shared.Enum;

namespace ProPhoneService.Api.Contracts;

/// <summary>
/// Запрос смены статуса заказа
/// </summary>
/// <param name="Status">Целевой статус</param>
/// <param name="Comment">Комментарий к смене статуса (необязательно)</param>
public record ChangeStatusRequest(
    RepairStatus Status,
    string? Comment);
