namespace ProPhoneService.Application.Contracts;

/// <summary>
/// Публичное представление услуги, включённой в заказ (цена зафиксирована на момент заказа)
/// </summary>
/// <param name="ServiceId">Идентификатор услуги из прайс-листа</param>
/// <param name="Name">Название услуги</param>
/// <param name="Price">Стоимость услуги на момент заказа, руб</param>
public record RepairOrderServiceDto(
    Guid ServiceId,
    string Name,
    decimal Price);
