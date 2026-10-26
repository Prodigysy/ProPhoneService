using Microsoft.AspNetCore.Mvc;
using ProPhoneService.Application.Contracts;
using ProPhoneService.Application.Services;

namespace ProPhoneService.Api.Controllers;

/// <summary>
/// Прайс-лист мастерской
/// </summary>
[ApiController]
[Route("api/services")]
public class ServicesController(PriceListService priceList) : ControllerBase
{
    /// <summary>
    /// Получить активные услуги прайс-листа
    /// </summary>
    /// <remarks>Список кэшируется на 5 минут; кэш сбрасывается при правке прайса</remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var services = await priceList.GetActiveAsync(cancellationToken);

        return Ok(services);
    }
}
