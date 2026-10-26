namespace ProPhoneService.Domain.Exceptions;

/// <summary>
/// Нарушение ограничения уникальности при сохранении (например, параллельная вставка одного и того же контакта двумя запросами)
/// </summary>
/// <param name="message">Сообщение об ошибке</param>
/// <param name="innerException">Исходное исключение провайдера данных</param>
public class UniqueConstraintViolationException(string message, Exception innerException) : Exception(message, innerException)
{
}
