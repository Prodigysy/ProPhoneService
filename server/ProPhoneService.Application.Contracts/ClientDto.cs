namespace ProPhoneService.Application.Contracts;

/// <summary>
/// Публичное представление клиента для карточки заказа
/// </summary>
/// <param name="Id">Идентификатор клиента</param>
/// <param name="Name">Имя клиента</param>
/// <param name="Phone">Телефон клиента (может отсутствовать)</param>
/// <param name="Email">Email клиента (может отсутствовать)</param>
public record ClientDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Email);
