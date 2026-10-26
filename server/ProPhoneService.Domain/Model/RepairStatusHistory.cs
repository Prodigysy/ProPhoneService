using ProPhoneService.Domain.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProPhoneService.Domain.Model;

/// <summary>
/// Запись истории смены статуса заказа на ремонт
/// </summary>
[Table("repair_status_history")]
public class RepairStatusHistory
{
    /// <summary>
    /// Уникальный идентификатор записи истории
    /// </summary>
    [Key]
    [Column("id")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Идентификатор заказа, к которому относится запись
    /// </summary>
    [ForeignKey(nameof(RepairOrderId))]
    [Column("repair_order_id")]
    public required Guid RepairOrderId { get; set; }

    /// <summary>
    /// Статус, в который перешёл заказ
    /// </summary>
    [Column("status")]
    public required RepairStatus Status { get; set; }

    /// <summary>
    /// Дата и время смены статуса (UTC)
    /// </summary>
    [Column("changed_at")]
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Комментарий к смене статуса (необязательно)
    /// </summary>
    [Column("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Заказ, к которому относится запись в истории
    /// </summary>
    public RepairOrder? RepairOrder { get; set; }
}
