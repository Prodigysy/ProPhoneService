namespace ProPhoneService.Application.Services;

/// <summary>
/// Результат смены статуса заказа
/// </summary>
public enum ChangeStatusResult
{
    /// <summary>
    /// Статус изменён, история дописана
    /// </summary>
    Success,

    /// <summary>
    /// Заказ не найден
    /// </summary>
    NotFound,

    /// <summary>
    /// Переход запрещён правилами машины состояний
    /// </summary>
    InvalidTransition,

    /// <summary>
    /// Заказ был изменён параллельно (конкурентная модификация)
    /// </summary>
    Conflict,
}
