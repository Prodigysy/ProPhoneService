using ProPhoneService.Domain.Exceptions;
using ProPhoneService.Domain.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProPhoneService.Domain.Model;

/// <summary>
/// Заказ на ремонт телефона в мастерской
/// </summary>
[Table("repair_order")]
public class RepairOrder : IEntity
{
    /// <summary>
    /// Уникальный идентификатор заказа
    /// </summary>
    [Key]
    [Column("id")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Идентификатор клиента, оформившего заказ
    /// </summary>
    [ForeignKey(nameof(ClientId))]
    [Column("client_id")]
    public required Guid ClientId { get; set; }

    /// <summary>
    /// Идентификатор устройства в ремонте
    /// </summary>
    [ForeignKey(nameof(DeviceId))]
    [Column("device_id")]
    public required Guid DeviceId { get; set; }

    /// <summary>
    /// Текущий статус заказа (<c>null</c> - заказ ещё не инициализирован через <see cref="Start"/>)
    /// </summary>
    [Column("status")]
    public RepairStatus? Status { get; private set; }

    /// <summary>
    /// Итоговая стоимость ремонта, руб
    /// </summary>
    [Column("total_price", TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Дата и время создания заказа (UTC)
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Предпочитаемая дата и время визита клиента (UTC)
    /// </summary>
    [Column("preferred_visit_at")]
    public DateTime? PreferredVisitAt { get; set; }

    /// <summary>
    /// Клиент, оформивший заказ
    /// </summary>
    public Client? Client { get; set; }

    /// <summary>
    /// Устройство в ремонте
    /// </summary>
    public Device? Device { get; set; }

    /// <summary>
    /// История смены статусов заказа
    /// </summary>
    public List<RepairStatusHistory> StatusHistory { get; set; } = [];

    /// <summary>
    /// Услуги, входящие в заказ
    /// </summary>
    public List<RepairOrderService> Services { get; set; } = [];

    /// <summary>
    /// Создать заказ и сразу установить начальный статус <see cref="RepairStatus.Created"/>
    /// </summary>
    /// <param name="clientId">Идентификатор клиента, оформившего заказ</param>
    /// <param name="deviceId">Идентификатор устройства в ремонте</param>
    /// <param name="preferredVisitAt">Предпочитаемая дата и время визита клиента (UTC, необязательно)</param>
    public static RepairOrder Create(Guid clientId, Guid deviceId, DateTime? preferredVisitAt = null)
    {
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            DeviceId = deviceId,
            PreferredVisitAt = preferredVisitAt,
        };

        order.Start();
        return order;
    }

    /// <summary>
    /// Установить начальный статус заказа (только один раз, при создании)
    /// </summary>
    /// <exception cref="InvalidOperationException">Статус уже установлен</exception>
    public void Start()
    {
        if (Status is not null)
        {
            throw new InvalidOperationException("Начальный статус заказа уже установлен");
        }

        Apply(RepairStatus.Created, comment: null);
    }

    /// <summary>
    /// Сменить статус заказа по правилам машины состояний
    /// </summary>
    /// <param name="next">Целевой статус</param>
    /// <param name="comment">Комментарий к смене статуса</param>
    /// <exception cref="InvalidStatusTransitionException">Заказ не инициализирован или переход из текущего статуса в целевой запрещён</exception>
    public void ChangeStatus(RepairStatus next, string? comment = null)
    {
        if (Status is null)
        {
            throw new InvalidStatusTransitionException("Заказ не инициализирован: сначала Start()");
        }

        if (!RepairStatusTransitions.IsAllowed(Status.Value, next))
        {
            throw new InvalidStatusTransitionException(
                $"Недопустимый переход статуса заказа: {Status} -> {next}");
        }

        Apply(next, comment);
    }

    /// <summary>
    /// Применить статус: обновить поле и дописать запись в историю
    /// </summary>
    private void Apply(RepairStatus next, string? comment)
    {
        Status = next;
        StatusHistory.Add(new RepairStatusHistory
        {
            Id = Guid.Empty,
            RepairOrderId = Id,
            Status = next,
            Comment = comment,
        });
    }
}
