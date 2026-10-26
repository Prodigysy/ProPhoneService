using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProPhoneService.Api.Contracts;
using ProPhoneService.Application.Contracts;
using ProPhoneService.Application.Services;
using ProPhoneService.Domain.Shared.Enum;

namespace ProPhoneService.Api.Controllers;

/// <summary>
/// Заказы на ремонт (панель мастера)
/// </summary>
[ApiController]
[Route("api/orders")]
public class OrdersController(OrderWorkflowService ordersService, OrderCreationService orderCreationService) : ControllerBase
{
    /// <summary>
    /// Получить страницу заказов с фильтром по статусу (новые сверху)
    /// </summary>
    /// <param name="status">Фильтр по статусу; без параметра — все заказы</param>
    /// <param name="page">Номер страницы, начиная с 1 (по умолчанию 1)</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RepairOrderListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RepairOrderListItemDto>>> GetPage(
        [FromQuery] RepairStatus? status = null,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || page > OrderWorkflowService.MaxPage)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Некорректный номер страницы",
                Detail = "Параметр page должен быть целым числом, начиная с 1",
            });
        }

        var pageItems = await ordersService.GetPageAsync(status, page, cancellationToken);

        return Ok(pageItems);
    }

    /// <summary>
    /// Получить карточку заказа: клиент, устройство, услуги и тайм-лайн статусов
    /// </summary>
    /// <param name="id">Идентификатор заказа</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RepairOrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepairOrderDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var card = await ordersService.GetByIdAsync(id, cancellationToken);

        if (card is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Заказ не найден",
                Detail = "Заказ не существует",
            });
        }

        return Ok(card);
    }

    /// <summary>
    /// Сменить статус заказа по правилам машины состояний
    /// </summary>
    /// <remarks>Запрещённый переход (перепрыгнуть этап или вернуть назад) возвращает 409</remarks>
    /// <param name="id">Идентификатор заказа</param>
    /// <param name="request">Целевой статус и комментарий</param>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await ordersService.ChangeStatusAsync(id, request.Status, request.Comment, cancellationToken);

        return result switch
        {
            ChangeStatusResult.NotFound => NotFound(new ProblemDetails
            {
                Title = "Заказ не найден",
                Detail = "Заказ не существует",
            }),
            ChangeStatusResult.InvalidTransition => Conflict(new ProblemDetails
            {
                Title = "Запрещённый переход статуса",
                Detail = $"Переход в статус {request.Status} недопустим по правилам машины состояний",
            }),
            ChangeStatusResult.Conflict => Conflict(new ProblemDetails
            {
                Title = "Заказ был изменён",
                Detail = "Заказ изменён другим пользователем, обновите страницу и повторите",
            }),
            ChangeStatusResult.Success => NoContent(),
            _ => throw new UnreachableException(),
        };
    }

    /// <summary>
    /// Создать заявку на ремонт
    /// </summary>
    /// <param name="request">Данные заявки</param>
    [HttpPost]
    [ProducesResponseType<CreateOrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateOrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await orderCreationService.CreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            var details = new ValidationProblemDetails(
                result.Errors!.ToDictionary(kv => kv.Key, kv => kv.Value));

            return ValidationProblem(details);
        }

        var orderId = result.OrderId!.Value;

        return CreatedAtAction(nameof(GetById), new { id = orderId }, new CreateOrderResponse(orderId));
    }
}
