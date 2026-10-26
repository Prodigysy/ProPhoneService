namespace ProPhoneService.Application.Contracts;

/// <summary>
/// Ответ на создание заявки: номер (идентификатор) заявки
/// </summary>
/// <param name="Id">Номер заявки</param>
public record CreateOrderResponse(Guid Id);
