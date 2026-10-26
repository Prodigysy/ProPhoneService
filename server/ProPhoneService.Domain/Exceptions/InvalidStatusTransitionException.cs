namespace ProPhoneService.Domain.Exceptions;

/// <summary>
/// Попытка смены статуса заказа в нарушение правил машины состояний:
/// заказ не инициализирован или переход в целевой статус запрещён
/// </summary>
/// <param name="message">Описание нарушения</param>
public class InvalidStatusTransitionException(string message) : Exception(message)
{
}
