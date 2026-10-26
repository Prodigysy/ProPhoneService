using System.ComponentModel.DataAnnotations;
using ProPhoneService.Domain.Shared.Enum;

namespace ProPhoneService.Application.Contracts;

/// <summary>
/// Запрос на создание заявки на ремонт
/// </summary>
public record CreateOrderRequest
{
    /// <summary>
    /// Имя клиента
    /// </summary>
    [Required(ErrorMessage = "Укажите имя")]
    [StringLength(200, ErrorMessage = "Имя слишком длинное")]
    public string Name { get; init; } = "";

    /// <summary>
    /// Телефон клиента в формате +7XXXXXXXXXX
    /// </summary>
    [RegularExpression(@"^\+7\d{10}$", ErrorMessage = "Телефон должен быть в формате +7XXXXXXXXXX")]
    public string? Phone { get; init; }

    /// <summary>
    /// Email клиента (необязательно)
    /// </summary>
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string? Email { get; init; }

    /// <summary>
    /// Тип устройства
    /// </summary>
    [Required(ErrorMessage = "Укажите тип устройства")]
    public DeviceType? DeviceType { get; init; }

    /// <summary>
    /// Производитель устройства
    /// </summary>
    [Required(ErrorMessage = "Укажите производителя устройства")]
    [StringLength(200, ErrorMessage = "Производитель устройства слишком длинный")]
    public string DeviceManufacturer { get; init; } = "";

    /// <summary>
    /// Модель устройства
    /// </summary>
    [Required(ErrorMessage = "Укажите модель устройства")]
    [StringLength(200, ErrorMessage = "Модель устройства слишком длинная")]
    public string DeviceModel { get; init; } = "";

    /// <summary>
    /// Желаемая дата и время визита (с учётом таймзоны)
    /// </summary>
    [Required(ErrorMessage = "Укажите желаемую дату и время визита")]
    public DateTimeOffset? PreferredVisitAt { get; init; }

    /// <summary>
    /// Идентификаторы выбранных услуг (может быть пустым)
    /// </summary>
    public IReadOnlyList<Guid>? ServiceIds { get; init; }
}
