namespace ProPhoneService.Domain.Exceptions;

/// <summary>
/// Исключение конкурентной модификации: сущность была изменена или удалена
/// другим процессом между чтением и сохранением
/// </summary>
/// <param name="message">Сообщение об ошибке</param>
/// <param name="innerException">Исходное исключение провайдера данных</param>
public class ConcurrencyConflictException(string message, Exception innerException) : Exception(message, innerException)
{
}
