using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProPhoneService.Domain.Model;

/// <summary>
/// Отзыв клиента о работе мастерской
/// </summary>
[Table("review")]
public class Review : IEntity
{
    /// <summary>
    /// Уникальный идентификатор отзыва
    /// </summary>
    [Key]
    [Column("id")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Идентификатор клиента, оставившего отзыв
    /// </summary>
    [ForeignKey(nameof(ClientId))]
    [Column("client_id")]
    public required Guid ClientId { get; set; }

    /// <summary>
    /// Оценка мастера (1–5)
    /// </summary>
    [Column("rating")]
    public int Rating { get; set; }

    /// <summary>
    /// Текст отзыва (необязательно)
    /// </summary>
    [Column("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Дата и время создания отзыва (UTC)
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Клиент, оставивший отзыв
    /// </summary>
    public Client? Client { get; set; }
}
