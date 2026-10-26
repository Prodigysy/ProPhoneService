namespace ProPhoneService.Application.Contracts;

/// <summary>
/// Публичное представление услуги из прайс-листа мастерской
/// </summary>
/// <param name="Id">Идентификатор услуги (для выбора при создании заявки)</param>
/// <param name="Name">Название услуги</param>
/// <param name="Price">Стоимость услуги, руб</param>
/// <param name="Description">Описание услуги (может отсутствовать)</param>
public record ServiceDto(
    Guid Id,
    string Name,
    decimal Price,
    string? Description);
