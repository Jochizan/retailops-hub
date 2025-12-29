using Microsoft.AspNetCore.Mvc;
using RetailOps.Application.Interfaces;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    [HttpGet("stock-critical")]
    public async Task<IActionResult> GetCriticalStock([FromQuery] int? storeId, [FromQuery] int limit = 10)
    {
        var result = await _reportsService.GetCriticalStockAsync(storeId, limit);
        return Ok(result);
    }

    [HttpGet("orders-summary")]
    public async Task<IActionResult> GetOrdersSummary([FromQuery] int? storeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _reportsService.GetOrdersSummaryAsync(storeId, from, to);
        return Ok(result);
    }

    [HttpGet("top-skus")]
    public async Task<IActionResult> GetTopSkus([FromQuery] int? storeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int limit = 5)
    {
        var result = await _reportsService.GetTopSkusAsync(storeId, from, to, limit);
        return Ok(result);
    }
}
